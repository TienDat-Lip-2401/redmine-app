<script setup>
import { computed } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "@/stores/authStore";
import MenuIcon from "@/assets/icons/Menu.svg";
import BaseMenu from "@/components/common/BaseMenu.vue";
const authStore = useAuthStore();
const router = useRouter();
const props = defineProps({
  project: {
    type: Object,
    required: true,
  },
});
const emit = defineEmits(["edit", "delete"]);
const currentUser = computed(() => authStore.user);
const hasPermission = computed(() => {
  if (!currentUser.value) return false;
  const isAdmin = currentUser.value.positions?.includes("Admin");
  const isProjectManager = currentUser.value.id === props.project.projectManagerId;
  return isAdmin || isProjectManager;
});
const handleEdit = () => {
  router.push({
    name: "UpdateProject",
    params: { id: props.project.id },
  });
  console.log("Edit project with ID:", props.project.id);
  emit("edit", props.project.id);
};

const handleDelete = () => {
  emit("delete", props.project.id);
};
const formatDate = (dateString) => {
  if (!dateString) return "";
  const date = new Date(dateString);
  const day = String(date.getDate()).padStart(2, "0");
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const year = date.getFullYear();
  return `${day}/${month}/${year}`;
};
const STATUS_COMPLETED = 2;
const DAY_MS = 24 * 60 * 60 * 1000;
// Mốc 0h theo giờ địa phương, để tính chênh lệch theo ngày (không theo giờ)
const startOfDay = (value) => {
  const d = new Date(value);
  return new Date(d.getFullYear(), d.getMonth(), d.getDate());
};
// Math.round để không lệch 1 ngày khi đi qua giờ mùa hè/đông
const daysBetween = (from, to) => Math.round((to - from) / DAY_MS);

// Tiến độ theo thời gian: số liệu thật duy nhất có được khi chưa có module Task
const timeline = computed(() => {
  if (Number(props.project.status) === STATUS_COMPLETED) return { kind: "completed", percent: 100 };
  const start = startOfDay(props.project.startDate);
  const end = startOfDay(props.project.endDate);
  const today = startOfDay(Date.now());
  if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) return null;
  if (today < start) return { kind: "upcoming", percent: 0 };
  if (today > end) return { kind: "overdue", percent: 100, days: daysBetween(end, today) };
  const totalDays = Math.max(1, daysBetween(start, end));
  const percent = Math.min(100, Math.round((daysBetween(start, today) / totalDays) * 100));
  return { kind: "running", percent, days: daysBetween(today, end) };
});
const getStatusLabel = (status) => {
  const statusMap = {
    0: "Active",
    1: "Pending",
    2: "Completed",
  };
  return statusMap[Number(status)] || "Active";
};
</script>

<template>
  <div class="project-card">
    <!-- Header -->
    <div class="card-header">
      <div class="title-group">
        <h3>{{ project.title }}</h3>
        <span class="project-id">{{ project.projectCode }}</span>
      </div>

      <BaseMenu v-if="hasPermission">
        <template #trigger>
          <button class="btn-more">
            <img :src="MenuIcon" alt="Menu" />
          </button>
        </template>

        <div class="menu-item edit" @click="handleEdit">{{ $t('common.edit') }}</div>
        <div class="menu-item delete" @click="handleDelete">{{ $t('common.delete') }}</div>
      </BaseMenu>
    </div>
    <!-- Body -->
    <div class="card-body">
      <p class="pm-text" data-testid="project-manager">
        {{ $t('projects.manager') }}: <span>{{ project.projectManagerName || '—' }}</span>
      </p>
      <div class="members-section">
        <span class="members-label">{{ $t('projects.members') }}:</span>
        <div class="avatar-group">
          <div
            v-for="(member, index) in project.members.slice(0, 4)"
            :key="member.userId"
            class="avatar-item"
            :title="member.userName"
            :style="{ zIndex: 10 - index }"
          >
            <img v-if="member.avatar" :src="member.avatar" :alt="member.userName" />
            <div v-else class="avatar-placeholder" :class="'color-' + (index % 4)">
              {{ member.userName.trim().split(/\s+/).pop().charAt(0).toUpperCase() }}
            </div>
          </div>

          <div v-if="project.members.length > 4" class="avatar-more">
            +{{ project.members.length - 4 }}
          </div>
        </div>
      </div>
      <!-- Số liệu Nhiệm vụ/Lỗi sẽ thêm khi có module Task; hiện chỉ hiện tiến độ theo thời gian -->
      <div v-if="timeline" class="stat-list">
        <div class="stat-item" data-testid="timeline" :data-kind="timeline.kind">
          <div class="stat-label">
            <span>{{ $t('projects.timeline_progress') }}:</span>
            <span :class="['timeline-text', timeline.kind]">
              {{ $t(`projects.timeline_${timeline.kind}`, { percent: timeline.percent, days: timeline.days }) }}
            </span>
          </div>
          <div
            class="progress-bar"
            role="progressbar"
            :aria-valuenow="timeline.percent"
            aria-valuemin="0"
            aria-valuemax="100"
          >
            <div :class="['progress-fill', timeline.kind]" :style="{ width: timeline.percent + '%' }"></div>
          </div>
        </div>
      </div>
    </div>
    <!-- Footer -->
    <div class="card-footer">
      <span class="status-badge" :class="getStatusLabel(project.status)">
        {{ $t('common.status.' + getStatusLabel(project.status).toLowerCase()) }}
      </span>
      <span class="timeline"
        >{{ $t('common.timeline') }}:
        <span class="time"
          >{{ formatDate(project.startDate) }} - {{ formatDate(project.endDate) }}</span
        ></span
      >
    </div>
  </div>
</template>

<style scoped>
.project-card {
  background-color: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}
.project-card:hover {
  transform: translateY(-5px);
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 20px;
}
.title-group {
  display: flex;
  flex-direction: column;
  .h3 {
    font-size: 26px;
    font-weight: 600;
    margin: 0;
  }
  .project-id {
    font-size: 12px;
    font-weight: 500;
    color: #888;
  }
}
.btn-more {
  width: 24px;
  height: 24px;
  background: none;
  border: none;
  border-radius: 50%;
  cursor: pointer;
  color: #000000;
  .img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }
}
.btn-more:hover {
  background-color: #e5e5e5;
  transform: scale(1.05);
}

.btn-more:active {
  transform: scale(0.95);
}
.pm-text {
  font-size: 12px;
  font-weight: 700;
  margin-bottom: 12px;
  color: #787486;
}

.pm-text span {
  font-weight: 500;
}
.members-section {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 20px;
}
.members-label {
  font-size: 12px;
  font-weight: 700;
  color: #787486;
}
.avatar-group {
  display: flex;
  align-items: center;
}
.avatar-item {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  border: 2px solid #fff;
  margin-left: -8px;
  overflow: hidden;
  background-color: #eee;
}
.avatar-item:first-child {
  margin-left: 0;
}
.avatar-item img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.avatar-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 600;
  color: #fff;
}
/* Màu giả lập cho placeholder nếu không có ảnh */
.color-0 {
  background-color: #ff9f43;
}
.color-1 {
  background-color: #28c76f;
}
.color-2 {
  background-color: #ea5455;
}
.color-3 {
  background-color: #7367f0;
}
.avatar-more {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background-color: #fce1d1;
  color: #f2994a;
  font-size: 11px;
  font-weight: 600;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-left: -8px;
  border: 2px solid #fff;
  z-index: 1;
}
/* Stats */
.stat-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-bottom: 20px;
}
.stat-label {
  display: flex;
  justify-content: space-between;
  font-size: 12px;
  font-weight: 500;
  color: #787486;
  margin-bottom: 6px;
}
.progress-bar {
  height: 16px;
  background: #f0f0f0;
  border-radius: 4px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: #dc3545;
  border-radius: 4px;
}
.progress-fill.overdue {
  background: #7f1d1d;
}
.progress-fill.completed {
  background: #16a34a;
}
.timeline-text.overdue {
  color: #b91c1c;
  font-weight: 700;
}
.timeline-text.completed {
  color: #15803d;
}
.card-footer {
  display: flex;
  align-items: center;
  justify-content: flex-start;
  gap: 20px;
  padding-top: 15px;
  border-top: 1px solid #f5f5f5;
}
.status-badge {
  padding: 4px 12px;
  border-radius: 100px;
  font-size: 15px;
  font-weight: 400;
}
.status-badge.Active {
  background: #198754;
  color: #ffffff;
}
.status-badge.Pending {
  background: #f40505;
  color: #fefefe;
}
.status-badge.Completed {
  background: #fdfeff;
  color: #0c6d97;
}
.timeline {
  font-size: 12px;
  font-weight: 700;
  color: #787486;
  .time {
    font-weight: 500;
  }
}
</style>
