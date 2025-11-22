<template>
  <div :class="['user-properties', className]">
    <div class="content flex items-center gap-8">
      <div class="content-border" />
      <div class="order-lbl">
        <div class="text-wrapper-2">Пользователи</div>
      </div>
      <div class="filters-border" />
      <div class="filters flex items-center gap-4 ml-auto">
        <div class="status-lbl">
          <div class="text-wrapper-3">Тип:</div>
        </div>
        <select v-model="localType" class="status">
          <option value="Все">Все</option>
          <option value="User">Покупатель</option>
          <option value="Seller">Продавец</option>
        </select>
        <div class="date-lbl">
          <div class="text-wrapper-3">Имя:</div>
        </div>
        <input v-model="localName" class="user-name" placeholder="Поиск по имени" />
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
  import { ref, watch } from 'vue';
  defineProps<{ className?: string }>();
  const localType = ref('Все');
  const localName = ref('');
  const emit = defineEmits(['filter']);
  watch([localType, localName], () => {
    emit('filter', { type: localType.value, name: localName.value });
  });
</script>
<style scoped>
  .user-properties {
    background-color: var(--color-background);
    border: 6px solid var(--color-background-border);
    border-radius: var(--radius-main);
    padding: 20px;
  }

  .content {
    display: flex;
    align-items: center;
    gap: 20px;
  }

  select, input {
    background-color: var(--color-teal-accent);
    border: 6px solid var(--color-teal-border);
    border-radius: var(--radius-main);
    color: var(--color-text-primary);
    padding: 10px;
    font-family: var(--font-additional);
  }

  .status-lbl, .date-lbl {
    background-color: #ff00f6;
    border: 6px solid #890065;
    border-radius: var(--radius-main);
    padding: 10px 20px;
    color: white;
  }

  .content-border, .filters-border {
    height: 0;
    border-top: 2px solid var(--color-background-border);
    flex-grow: 1;
  }

  .order-lbl {
    background-color: var(--color-bg);
    border: 12px solid var(--color-background-border);
    border-radius: var(--radius-main);
    padding: 10px 20px;
  }
</style>
