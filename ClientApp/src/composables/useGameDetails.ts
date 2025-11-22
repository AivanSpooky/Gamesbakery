import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { GameService } from '@/services/GameService';
import { OrderItemService } from '@/services/OrderItemService';
import { SellerService } from '@/services/SellerService';
import { useNotifications } from '@/composables/useNotifications';
import type { Game, GameOrderItem } from '@/types/game';

export function useGameDetails() {
  const route = useRoute();
  const game = ref<Game | null>(null);
  const orderItems = ref<GameOrderItem[]>([]);
  const loading = ref(true);
  const error = ref('');
  const { showError } = useNotifications();

  onMounted(async () => {
    try {
      const id = route.params.id as string;
      game.value = await GameService.getById(id, true);
      orderItems.value = await OrderItemService.getByGameId(id) || [];
      for (const item of orderItems.value) {
        if (item.sellerId) {
          const seller = await SellerService.getById(item.sellerId);
          item.averageRating = seller.avgRating;
        }
      }
    } catch (err: any) {
      error.value = 'Ошибка загрузки данных игры';
      showError(error.value);
    } finally {
      loading.value = false;
    }
  });

  return { game, orderItems, loading, error };
}
