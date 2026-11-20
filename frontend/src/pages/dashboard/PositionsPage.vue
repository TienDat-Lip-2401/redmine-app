<script setup>
import CatalogManager from "@/components/catalog/CatalogManager.vue";
import { positionService } from "@/services/position";

const api = {
  list: positionService.getAllPosition,
  create: positionService.create,
  update: positionService.update,
  remove: positionService.remove,
};
</script>

<template>
  <CatalogManager ns="positions" :api="api" count-field="userCount" />
</template>
