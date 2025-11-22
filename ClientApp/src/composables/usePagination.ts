import { ref, computed, type Ref } from 'vue';

export function usePagination<T>(items: Ref<T[]>, perPage = 10) {
  const page = ref(1);

  const totalPages = computed(() => Math.ceil(items.value.length / perPage) || 1);

  const displayedItems = computed(() => {
    const start = (page.value - 1) * perPage;
    return items.value.slice(start, start + perPage);
  });

  const setPage = (p: number) => (page.value = p);

  return { page, totalPages, displayedItems, setPage };
}
