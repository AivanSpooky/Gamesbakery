<template>
  <div class="admin-users view-container">
    <UserProperties @filter="applyFilters" />
    <div v-if="isLoading" class="loading-message">Loading...</div>
    <div v-else class="users-list flex flex-col gap-8 mt-20">
      <UserItemExample v-for="entity in displayedItems"
                       :key="entity.id"
                       :name="entity.username"
                       :detail="entity.type === 'User' ? (entity.email || 'N/A') : `Рейтинг: ${entity.avgRating || 'N/A'}`"
                       :action1-text="getAction1Text(entity)"
                       :action2-text="getAction2Text(entity)"
                       @action1="handleAction1(entity)"
                       @action2="handleAction2(entity)"
                       @more="handleMore(entity)" />
    </div>
    <div v-if="!isLoading" class="flex justify-center mt-32">
      <Pages :current-page="page" :total-pages="totalPages" @update:page="page = $event" />
    </div>
  </div>
</template>
<script setup lang="ts">
  import { computed } from 'vue';
  import { UserService } from '@/services/UserService';
  import UserItemExample from '@/components/ui/admin/UserItemExample.vue';
  import UserProperties from '@/components/ui/admin/UserProperties.vue';
  import Pages from '@/components/ui/pages/Pages.vue';
  import { useAdminEntities } from '@/composables/useAdminEntities';
  import { usePagination } from '@/composables/usePagination';
  import { useNotifications } from '@/composables/useNotifications';
  import type { AdminEntity } from '@/types/admin';
  const { filteredEntities, isLoading, applyFilters } = useAdminEntities();
  const { page, totalPages, displayedItems } = usePagination<AdminEntity>(filteredEntities);
  const { showSuccess, showError } = useNotifications();
  const getAction1Text = (entity: AdminEntity) => {
    if (entity.type === 'User') return entity.isBlocked ? '' : 'Забанить';
    return '';
  };
  const getAction2Text = (entity: AdminEntity) => {
    if (entity.type === 'User') return entity.isBlocked ? 'Разбанить' : '';
    return '';
  };
  const handleAction1 = async (entity: AdminEntity) => {
    if (entity.type === 'User' && !entity.isBlocked) {
      try {
        await UserService.banUser(entity.id);
        entity.isBlocked = true;
        showSuccess('Пользователь успешно забанен');
      } catch (err: any) {
        showError('Ошибка бана пользователя');
      }
    }
  };
  const handleAction2 = async (entity: AdminEntity) => {
    if (entity.type === 'User' && entity.isBlocked) {
      try {
        await UserService.unbanUser(entity.id);
        entity.isBlocked = false;
        showSuccess('Пользователь успешно разбанен');
      } catch (err: any) {
        showError('Ошибка разбана пользователя');
      }
    }
  };
  const handleMore = (entity: AdminEntity) => {
    console.log(`View details for ${entity.type} ${entity.id}`);
  };
</script>
<style scoped>
  .admin-users {
    position: relative;
    min-height: 100vh;
    overflow: hidden;
    background-color: var(--color-background);
    padding: 40px;
  }
</style>
