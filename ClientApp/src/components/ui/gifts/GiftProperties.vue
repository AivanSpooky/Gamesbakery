<template>
  <div :class="['gift-properties', className]">
    <OrderTitle :text="title" />
    <div class="filters flex items-center gap-4">
      <StatusLabel :text="typeLblText" />
      <TypeSelect v-model="localType" />
      <FilterButton @filter="emitFilter" />
    </div>
  </div>
</template>
<script setup lang="ts">
  import { ref } from 'vue';
  import OrderTitle from '@/components/ui/orders/OrderTitle.vue';
  import StatusLabel from '@/components/ui/orders/StatusLabel.vue';
  import TypeSelect from '@/components/ui/gifts/TypeSelect.vue'; // New select component
  import FilterButton from '@/components/ui/filters/FilterButton.vue'; // Assume exists
  defineProps<{
    className?: string;
    title: string;
    typeLblText: string;
  }>();
  const localType = ref('All');
  const emit = defineEmits(['filter']);
  const emitFilter = () => {
    emit('filter', { type: localType.value === 'All' ? 'all' : localType.value.toLowerCase() });
  };
</script>
<style scoped>
  .gift-properties {
    background-color: var(--color-background);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    flex-direction: column;
    width: 100%;
  }

  .filters {
    display: flex;
    align-items: center;
    gap: 20px;
    padding: 10px 20px;
  }
</style>
