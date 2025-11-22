import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { CartService } from '@/services/CartService';
import { OrderService } from '@/services/OrderService';
import { UserService } from '@/services/UserService';
import { useNotifications } from '@/composables/useNotifications';
import type { CartItem } from '@/types/cart';

export function useCart() {
  const cart = ref<CartItem[]>([]);
  const total = computed(() => CartService.getTotal(cart.value));
  const router = useRouter();
  const { showSuccess, showError, showWarning } = useNotifications();

  const loadCart = async () => {
    try {
      cart.value = await CartService.getItems();
    } catch (err: any) {
      showError('Ошибка загрузки корзины');
    }
  };

  const removeFromCart = async (orderItemId: string) => {
    try {
      await CartService.removeItem(orderItemId);
      await loadCart();
      showSuccess('Товар удалён из корзины');
    } catch (err: any) {
      showError('Ошибка удаления товара');
    }
  };

  const clearCart = async () => {
    try {
      await CartService.clear();
      cart.value = [];
      showSuccess('Корзина очищена');
    } catch (err: any) {
      showError('Ошибка очистки корзины');
    }
  };

  const checkout = async () => {
    try {
      if (cart.value.length === 0) {
        showWarning('Корзина пуста');
        return;
      }
      const user = await UserService.getProfile();
      if (user.balance < total.value) {
        showError('Недостаточно средств на балансе');
        return;
      }
      const cartItemIds = cart.value.map(item => item.orderItemId);
      await OrderService.checkout(cartItemIds);
      cart.value = [];
      showSuccess('Заказ успешно оформлен!');
      router.push('/order/index');
    } catch (err: any) {
      showError('Ошибка оформления заказа');
    }
  };

  onMounted(loadCart);
  return { cart, total, removeFromCart, clearCart, checkout };
}
