<script setup>
import CatalogManager from "@/components/catalog/CatalogManager.vue";
import { roleService } from "@/services/role";
import { useRoleStore } from "@/stores/roleStore";

const roleStore = useRoleStore();
const api = {
  list: roleService.getAll,
  create: roleService.create,
  update: roleService.update,
  remove: roleService.remove,
};
</script>

<template>
  <!-- Làm mới roleStore để dropdown vai trò ở màn dự án thấy ngay thay đổi -->
  <CatalogManager ns="roles" :api="api" count-field="memberCount" @changed="roleStore.fetchRoles(true)" />
</template>
