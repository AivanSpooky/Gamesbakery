<template>
  <div :class="['pages', className]">
    <OtherPage v-if="currentPage > 1" text="&lt;" @click="$emit('update:page', currentPage - 1)" />
    <OtherPage text="1" :selected="currentPage === 1" @click="$emit('update:page', 1)" />
    <OtherPage v-if="currentPage > 3" text="..." :disabled="true" />
    <template v-for="p in pagesToShow" :key="p">
      <OtherPage :text="p.toString()" :selected="currentPage === p" @click="$emit('update:page', p)" /> <!-- Added .toString() -->
    </template>
    <OtherPage v-if="currentPage < totalPages - 2" text="..." :disabled="true" />
    <OtherPage :text="totalPages.toString()" :selected="currentPage === totalPages" @click="$emit('update:page', totalPages)" /> <!-- Added .toString() -->
    <OtherPage v-if="currentPage < totalPages" text="&gt;" @click="$emit('update:page', currentPage + 1)" />
  </div>
</template>
<script lang="ts">
  import { defineComponent, computed } from 'vue';
  import OtherPage from './OtherPage.vue';

  export default defineComponent({
    name: 'Pages',
    components: { OtherPage },
    props: {
      className: { type: String, default: '' },
      currentPage: { type: Number, default: 1 },
      totalPages: { type: Number, default: 1 },
    },
    emits: ['update:page'],
    setup(props) {
      const pagesToShow = computed(() => {
        const start = Math.max(2, props.currentPage - 1);
        const end = Math.min(props.totalPages - 1, props.currentPage + 1);
        return Array.from({ length: end - start + 1 }, (_, i) => start + i);
      });
      return { pagesToShow };
    },
  });
</script>
<style scoped>
  .pages {
    align-items: flex-end;
    background-color: var(--color-background);
    display: flex;
    gap: 6px;
    padding: 11px;
    width: 100%;
    max-width: 1662px;
    justify-content: center;
  }
</style>
