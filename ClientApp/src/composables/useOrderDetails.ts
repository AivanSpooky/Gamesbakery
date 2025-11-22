import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { OrderService } from '@/services/OrderService';
import { OrderItemService } from '@/services/OrderItemService';
import { useNotifications } from '@/composables/useNotifications';
import type { Order, OrderItem } from '@/types/order';

export function useOrderDetails() {
  const route = useRoute();
  const order = ref<Order | null>(null);
  const revealedKeys = ref<Set<string>>(new Set());
  const loading = ref(true);
  const error = ref('');
  const { showWarning, showError } = useNotifications();

  const toggleKey = async (itemId: string) => {
    try {
      if (!revealedKeys.value.has(itemId)) {
        const key = await OrderItemService.getKeyById(itemId);
        const item = order.value?.items.find(i => i.id === itemId);
        if (item) item.key = key;
        revealedKeys.value.add(itemId);
        showWarning('Ключ открыт!');
      }
    } catch (err: any) {
      showError('Не удалось получить ключ');
    }
  };

  onMounted(async () => {
    try {
      const id = route.params.id as string;
      const rawOrder = await OrderService.getById(id);
      order.value = {
        id: rawOrder.id.toString(),
        totalPrice: rawOrder.totalPrice,
        orderDate: rawOrder.orderDate,
        status: rawOrder.isCompleted ? 'Completed' : (rawOrder.isOverdue ? 'Overdue' : 'Pending'),
        items: (rawOrder.orderItems || []).map((oi: any) => ({
          id: oi.id.toString(),
          gameTitle: oi.gameTitle,
          sellerName: oi.sellerName,
        })),
      };
    } catch (err: any) {
      error.value = 'Ошибка загрузки заказа';
      showError(error.value);
    } finally {
      loading.value = false;
    }
  });

  return { order, revealedKeys, toggleKey, loading, error };
}
