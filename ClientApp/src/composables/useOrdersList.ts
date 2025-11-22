import { ref, computed, onMounted } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { OrderService } from '@/services/OrderService';
import { SellerService } from '@/services/SellerService';
import { useNotifications } from '@/composables/useNotifications';
import type { OrderItem } from '@/types/order';

export function useOrdersList() {
  const auth = useAuthStore();
  const isSeller = computed(() => auth.role === 'Seller');
  const items = ref<OrderItem[]>([]);
  const loading = ref(true);
  const error = ref('');
  const { showError } = useNotifications();

  onMounted(async () => {
    try {
      if (!isSeller.value) {
        const rawOrders = await OrderService.getAll({ page: 1, limit: 100 });
        items.value = (rawOrders.items || []).map((o: any) => ({
          id: o.orderId.toString(),
          totalAmount: o.totalAmount,
          orderDate: o.orderDate,
          status: o.isCompleted ? 'Завершен' : (o.isOverdue ? 'Просрочен' : 'В ожидании'),
        }));
      } else {
        const rawItems = await SellerService.getOrderItems({ page: 1, limit: 100 });
        items.value = (rawItems.items || []).map((o: any) => ({
          id: o.id.toString(),
          gameTitle: o.gameTitle,
          gamePrice: o.gamePrice,
          status: o.orderId ? 'Продан' : 'Продается',
        }));
      }
      error.value = items.value.length ? '' : 'Нет элементов';
    } catch (err: any) {
      error.value = 'Ошибка загрузки данных';
      showError(error.value);
    } finally {
      loading.value = false;
    }
  });

  return { isSeller, items, loading, error };
}
