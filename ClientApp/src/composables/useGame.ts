import { ref, onMounted } from 'vue';
import { GameService } from '@/services/GameService';
import { useNotifications } from '@/composables/useNotifications';
import type { Game } from '@/types/game';

export function useGamesList() {
  const games = ref<Game[]>([]);
  const loading = ref(true);
  const error = ref<string | null>(null);
  const filters = ref<{ genre?: string; minPrice?: number; maxPrice?: number }>({});
  const { showError } = useNotifications();

  const loadGames = async () => {
    loading.value = true;
    error.value = null;
    try {
      games.value = await GameService.getAll(
        filters.value.genre || undefined,
        filters.value.minPrice,
        filters.value.maxPrice,
      );
    } catch (err: any) {
      error.value = 'Failed to load games';
      showError(error.value);
    } finally {
      loading.value = false;
    }
  };

  const applyFilters = (newFilters: { genre?: string; minPrice?: number; maxPrice?: number }) => {
    filters.value = { ...newFilters };
    loadGames();  // Reload with new filters
  };

  onMounted(loadGames);

  return { games, applyFilters, loading, error };
}
