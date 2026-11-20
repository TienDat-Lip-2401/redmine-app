import { defineStore } from "pinia";
import { ref } from "vue";
import { roleService } from "@/services/role";

export const useRoleStore = defineStore("role", () => {
  const roles = ref([]);
  const isLoaded = ref(false);

  // force = true để tải lại sau khi thêm/sửa/xoá
  const fetchRoles = async (force = false) => {
    if (isLoaded.value && !force) return roles.value;
    const res = await roleService.getAll();
    roles.value = res.data || [];
    isLoaded.value = true;
    return roles.value;
  };

  const getRoleName = (roleId) => roles.value.find((r) => r.id === Number(roleId))?.name || "Member";

  return { roles, fetchRoles, getRoleName };
});
