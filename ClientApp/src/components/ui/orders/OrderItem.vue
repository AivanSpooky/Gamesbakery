<template>
  <div :class="['order-item', className, good ? 'good' : 'bad']">
    <GoodRating v-if="good" :rating="rating" />
    <BadRating v-else :rating="rating" />
    <OrderItemSeller :seller="seller" />
    <PutToCartBtn :disabled="inCart" :class="{ 'disabled-btn': inCart }" @add-to-cart="addToCart" />
  </div>
</template>
<script lang="ts">
  import { defineComponent, ref, onMounted } from 'vue';
  import GoodRating from './GoodRating.vue';
  import BadRating from './BadRating.vue';
  import OrderItemSeller from './OrderItemSeller.vue';
  import PutToCartBtn from './PutToCartBtn.vue';
  import { CartService } from '@/services/CartService';
  import { useAuthStore } from '@/stores/auth';
  import { useRouter } from 'vue-router';
  import { useNotifications } from '@/composables/useNotifications';

  const { showSuccess, showWarning, showError } = useNotifications();
  export default defineComponent({
    name: 'OrderItem',
    components: { GoodRating, BadRating, OrderItemSeller, PutToCartBtn },
    props: {
      className: { type: String, default: '' },
      good: { type: Boolean, default: true },
      rating: { type: String, default: '4.72' },
      seller: { type: String, default: 'Seller-1' },
      id: { type: String, required: true },
    },
    setup(props) {
      const inCart = ref(false);
      const authStore = useAuthStore();
      const router = useRouter();

      onMounted(async () => {
        if (!authStore.isAuthenticated) return;
        try {
          const items = await CartService.getItems();
          inCart.value = items.some((item: any) => item.orderItemId === props.id);
        } catch (err) {
          console.error('Error checking cart:', err);
        }
      });

      const addToCart = async () => {
        if (!authStore.isAuthenticated) {
          showError('Для добавления ключа необходим вход!');
          console.log('Not authenticated, redirecting to login');
          router.push('/account/login');
          return;
        }
        if (inCart.value) {
          showWarning('Ключ уже в корзине!');
          console.log('Item already in cart');
          return;
        }
        try {
          await CartService.addItem(props.id);
          inCart.value = true;
          console.log('Item added to cart successfully');
          showSuccess('Ключ добавлен в корзину!');
        } catch (err) {
          showError('Ошибка добавления ключа в корзину!');
          console.error('Error adding to cart:', err);
        }
      };

      return { inCart, addToCart };
    },
  });
</script>
<style scoped>
  .order-item {
    background-color: var(--color-background);
    display: flex;
    gap: 17px;
    height: 110px;
    overflow: hidden;
    position: relative;
    width: 100%;
    max-width: 1662px;
    align-items: center;
    padding: 10px;
  }
  .disabled-btn {
    background-color: var(--color-money-accent);
    cursor: not-allowed;
  }
</style>
