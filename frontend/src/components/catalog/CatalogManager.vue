<script setup>
// Màn quản lý danh mục chỉ có trường "Tên" (dùng chung cho Chức vụ và Vai trò dự án).
// Nhãn lấy từ namespace i18n `ns`; `api` gồm list/create/update/remove trả về ApiResponse.
import { ref, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import BaseButton from "@/components/common/BaseButton.vue";
import BaseInput from "@/components/common/BaseInput.vue";
import BaseLoading from "@/components/common/BaseLoading.vue";
import ConfirmModal from "@/components/common/ConfirmModal.vue";
import { useModalStore } from "@/stores/modalStore";
import editIcon from "@/assets/icons/edit.svg";
import trashIcon from "@/assets/icons/trash.svg";

const props = defineProps({
  ns: { type: String, required: true },
  api: { type: Object, required: true },
  countField: { type: String, required: true },
});
const emit = defineEmits(["changed"]);

const { t } = useI18n();
const modalStore = useModalStore();

const items = ref([]);
const isLoading = ref(false);

// Form thêm/sửa: editingId = null nghĩa là thêm mới
const isFormOpen = ref(false);
const editingId = ref(null);
const formName = ref("");
const formError = ref("");
const isSaving = ref(false);

const isConfirmOpen = ref(false);
const itemToDelete = ref(null);

const serverMessage = (error, fallback) => error?.response?.data?.message || fallback;

const loadItems = async () => {
  isLoading.value = true;
  try {
    const res = await props.api.list();
    items.value = res.data || [];
  } catch (error) {
    modalStore.showModal({
      title: t("common.alert.failed"),
      message: serverMessage(error, t(`${props.ns}.load_failed`)),
      type: "error",
    });
  } finally {
    isLoading.value = false;
  }
};

const openCreate = () => {
  editingId.value = null;
  formName.value = "";
  formError.value = "";
  isFormOpen.value = true;
};

const openEdit = (item) => {
  editingId.value = item.id;
  formName.value = item.name;
  formError.value = "";
  isFormOpen.value = true;
};

const closeForm = () => {
  if (!isSaving.value) isFormOpen.value = false;
};

const submitForm = async () => {
  const name = formName.value.trim();
  if (!name) {
    formError.value = t(`${props.ns}.name_required`);
    return;
  }
  isSaving.value = true;
  formError.value = "";
  try {
    if (editingId.value === null) {
      await props.api.create({ name });
    } else {
      await props.api.update(editingId.value, { name });
    }
    isFormOpen.value = false;
    await loadItems();
    emit("changed");
  } catch (error) {
    // Lỗi nghiệp vụ (trùng tên, tên dành riêng...) hiển thị ngay trong form
    formError.value = serverMessage(error, t(`${props.ns}.save_failed`));
  } finally {
    isSaving.value = false;
  }
};

const triggerDelete = (item) => {
  itemToDelete.value = item;
  isConfirmOpen.value = true;
};

const confirmDelete = async () => {
  const item = itemToDelete.value;
  isConfirmOpen.value = false;
  itemToDelete.value = null;
  if (!item) return;
  isLoading.value = true;
  try {
    await props.api.remove(item.id);
    await loadItems();
    emit("changed");
  } catch (error) {
    modalStore.showModal({
      title: t("common.alert.failed"),
      message: serverMessage(error, t(`${props.ns}.delete_failed`)),
      type: "error",
    });
  } finally {
    isLoading.value = false;
  }
};

onMounted(loadItems);
</script>

<template>
  <div class="catalog-container">
    <div class="page-header">
      <div>
        <h1 class="page-title">{{ $t(`${ns}.title`) }}</h1>
        <p class="page-subtitle">{{ $t(`${ns}.subtitle`) }}</p>
      </div>
      <BaseButton variant="danger" class="btn-create" data-testid="catalog-create" @click="openCreate">
        + {{ $t(`${ns}.add`) }}
      </BaseButton>
    </div>

    <div class="table-card">
      <table class="catalog-table">
        <thead>
          <tr>
            <th class="col-no">{{ $t("common.no") }}</th>
            <th>{{ $t(`${ns}.name`) }}</th>
            <th class="col-count">{{ $t(`${ns}.count`) }}</th>
            <th class="col-action text-center">{{ $t("common.action") }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(item, index) in items" :key="item.id" :data-testid="`catalog-row-${item.id}`">
            <td>{{ index + 1 }}</td>
            <td class="item-name">
              {{ item.name }}
              <span v-if="item.isSystem" class="system-badge" :title="$t(`${ns}.system_hint`)">
                {{ $t(`${ns}.system`) }}
              </span>
            </td>
            <td>{{ item[countField] ?? 0 }}</td>
            <td>
              <div v-if="!item.isSystem" class="action-btns">
                <button class="btn-icon" :aria-label="$t('common.edit')" data-testid="catalog-edit" @click="openEdit(item)">
                  <img :src="editIcon" alt="" />
                </button>
                <button
                  class="btn-icon"
                  :aria-label="$t('common.delete')"
                  data-testid="catalog-delete"
                  @click="triggerDelete(item)"
                >
                  <img :src="trashIcon" alt="" />
                </button>
              </div>
              <div v-else class="action-locked">—</div>
            </td>
          </tr>
          <tr v-if="!isLoading && items.length === 0">
            <td colspan="4" class="empty-state">{{ $t(`${ns}.empty`) }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="isFormOpen" class="modal-overlay" @click.self="closeForm">
      <form class="form-card" data-testid="catalog-form" @submit.prevent="submitForm">
        <h2 class="form-title">{{ editingId === null ? $t(`${ns}.add`) : $t(`${ns}.edit`) }}</h2>
        <BaseInput
          id="catalog-name"
          v-model="formName"
          :label="$t(`${ns}.name`)"
          :placeholder="$t(`${ns}.name_placeholder`)"
          maxlength="100"
          required
          autofocus
        />
        <p v-if="formError" class="form-error" role="alert">{{ formError }}</p>
        <div class="form-actions">
          <BaseButton type="button" variant="outline" @click="closeForm">{{ $t("common.cancel") }}</BaseButton>
          <BaseButton type="submit" variant="danger" :disabled="isSaving">
            {{ isSaving ? $t("common.loading") : $t("common.save") }}
          </BaseButton>
        </div>
      </form>
    </div>

    <ConfirmModal
      :isOpen="isConfirmOpen"
      :message="$t(`${ns}.confirm_delete`, { name: itemToDelete?.name || '' })"
      @close="isConfirmOpen = false"
      @confirm="confirmDelete"
    />
    <BaseLoading :isLoading="isLoading" />
  </div>
</template>

<style scoped>
.catalog-container {
  padding: 2rem 3rem;
  background-color: #f8fafc;
  min-height: 100vh;
}
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 2rem;
  margin-bottom: 2.5rem;
}
.page-title {
  font-size: 3rem;
  font-weight: 700;
  color: #0e2040;
}
.page-subtitle {
  margin-top: 0.6rem;
  font-size: 1.5rem;
  color: #64748b;
}
.table-card {
  background: white;
  border-radius: 1rem;
  overflow: hidden;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
}
.catalog-table {
  width: 100%;
  text-align: left;
  border-collapse: collapse;
}
.catalog-table th {
  background-color: #f2f2f7;
  padding: 1.5rem;
  font-size: 1.8rem;
  font-weight: 700;
  color: #000;
}
.catalog-table td {
  padding: 1.5rem;
  font-size: 1.8rem;
  border-bottom: 1px solid #f1f5f9;
  color: #000;
}
.col-no {
  width: 8rem;
}
.col-count {
  width: 20rem;
}
.col-action {
  width: 14rem;
}
.text-center {
  text-align: center;
}
.item-name {
  font-weight: 500;
}
.system-badge {
  display: inline-block;
  margin-left: 1rem;
  padding: 0.2rem 1rem;
  font-size: 1.2rem;
  font-weight: 600;
  color: #1e40af;
  background: #dbeafe;
  border-radius: 999px;
  vertical-align: middle;
}
.action-btns {
  display: flex;
  justify-content: center;
  gap: 1.2rem;
}
.action-locked {
  text-align: center;
  color: #94a3b8;
}
.btn-icon {
  background: none;
  border: none;
  cursor: pointer;
  padding: 0.4rem;
  display: flex;
  align-items: center;
}
.btn-icon img {
  width: 16px;
  height: 16px;
  object-fit: contain;
}
.empty-state {
  text-align: center;
  color: #64748b;
}
.modal-overlay {
  position: fixed;
  inset: 0;
  background-color: rgba(0, 0, 0, 0.4);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9000;
}
.form-card {
  background: white;
  width: min(48rem, calc(100vw - 3.2rem));
  padding: 3rem;
  border-radius: 1.5rem;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
}
.form-title {
  font-size: 2.4rem;
  font-weight: 700;
  color: #0e2040;
  margin-bottom: 2rem;
}
.form-error {
  margin-top: -0.8rem;
  margin-bottom: 1.6rem;
  font-size: 1.4rem;
  color: #dc2626;
}
.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1.2rem;
}
</style>
