<template>
  <div :class="['game-filters', className]">
    <div class="filters">
      <div class="rectangle" />
      <div class="left-section">
        <GamesListTitle />
        <FilterButton @click="emitFilter" />
      </div>
      <div class="right-section">
        <div class="category-row">
          <CategoryLbl />
          <CategorySelect v-model="genre" />
        </div>
        <div class="price-row">
          <ToLbl text="От:" />
          <ToPrice v-model="minPrice" placeholder="49" />
          <ToLbl text="до" />
          <ToPrice v-model="maxPrice" placeholder="249" />
        </div>
      </div>
      <div class="background-accent" />
    </div>
  </div>
</template>
<script setup lang="ts">
  import { ref } from 'vue';
  import CategoryLbl from '../game/CategoryLbl.vue';
  import CategorySelect from '../game/CategorySelect.vue';
  import ToLbl from '../game/ToLbl.vue';
  import ToPrice from '../game/ToPrice.vue';
  import FilterButton from './FilterButton.vue';
  import GamesListTitle from './GamesListTitle.vue';
  defineProps<{
    className?: string;
  }>();
  const genre = ref<string>('All');
  const minPrice = ref<string>('');
  const maxPrice = ref<string>('');
  const emit = defineEmits(['filter']);
  const emitFilter = () => {
    const parsedMin = minPrice.value ? Number(minPrice.value) : null;
    const parsedMax = maxPrice.value ? Number(maxPrice.value) : null;
    emit('filter', {
      genre: genre.value === 'All' ? '' : genre.value,
      minPrice: parsedMin,
      maxPrice: parsedMax,
    });
  };
</script>
<style scoped>
  .game-filters {
    background-color: var(--color-background);
    display: flex;
    justify-content: center;
    height: 242px;
    position: relative;
    width: 100%;
  }

  .filters {
    height: 211px;
    margin-top: 20px;
    overflow: hidden;
    position: relative;
    width: 1640px;
    display: flex;
    gap: 20px;
    flex-wrap: nowrap;
    margin-left: auto;
    margin-right: auto;
  }

  .rectangle {
    background-color: var(--color-background);
    border: 6px solid var(--color-border-1);
    border-radius: var(--radius-main);
    height: 212px;
    left: 0;
    position: absolute;
    top: 0;
    width: 100%;
    z-index: 0;
  }

  .background-accent {
    background-color: var(--color-background-accent);
    border: 6px solid var(--color-border-1);
    border-radius: var(--radius-main);
    height: 210px;
    position: absolute;
    top: 3px;
    left: 522px;
    width: 1118px;
    z-index: 1;
  }

  .left-section {
    display: flex;
    flex-direction: column;
    gap: 10px;
    width: 511px;
    z-index: 2;
  }

  .right-section {
    display: flex;
    flex-direction: column;
    gap: 15px;
    flex: 1;
    z-index: 2;
    margin-top: 10px;
    margin-right: 9px;
  }

  .category-row {
    display: flex;
    gap: 11px;
    justify-content: flex-start;
    align-self: flex-end;
  }

  .price-row {
    display: flex;
    gap: 11px;
    justify-content: flex-start;
    align-self: flex-end;
  }
</style>
