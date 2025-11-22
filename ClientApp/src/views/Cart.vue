<template>
  <div class="cart-screen view-container">
    <div class="container max-w-full mx-auto px-8 pt-32">
      <div class="welcome-screen">
        <BigTitle text="Корзина" class="max-w-1662 mx-auto" />
        <div class="cart-items mt-20 max-w-1662 mx-auto">
          <CartItemRow v-for="item in displayedItems" :key="item.id" :game-text="item.gameTitle" :price-text="item.gamePrice + ' ₽'" :seller-text="item.sellerName" @delete="removeFromCart(item.orderItemId)" />
        </div>
        <TotalSum :total="total" class="mt-32 max-w-1662 mx-auto" />
        <ActionsExample class="mt-32 max-w-1662 mx-auto" haveAccLblText="Очистить корзину" regLblText="Оформить заказ" @haveAccClick="clearCart" @regClick="checkout" />
        <div class="flex justify-center mt-32 max-w-1662 mx-auto">
          <Pages :current-page="page" :total-pages="totalPages" @update:page="page = $event" />
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
  import BigTitle from '@/components/ui/titles/BigTitle.vue';
  import CartItemRow from '@/components/ui/cart/CartItemRow.vue';
  import TotalSum from '@/components/ui/cart/TotalSum.vue';
  import ActionsExample from '@/components/ui/ActionsExample.vue';
  import Pages from '@/components/ui/pages/Pages.vue';
  import { useCart } from '@/composables/useCart';
  import { usePagination } from '@/composables/usePagination';
  import type { CartItem } from '@/types/cart';
  const { cart, total, removeFromCart, clearCart, checkout } = useCart();
  const { page, totalPages, displayedItems } = usePagination<CartItem>(cart);
</script>

<style scoped>
  .cart-screen {
    position: relative;
    height: 100vh;
    overflow: auto;
    background-color: var(--color-background);
    display: flex;
    justify-content: center;
    align-items: flex-start;
  }

  .bg {
    aspect-ratio: 1.5;
    height: 1404px;
    left: -79px;
    position: absolute;
    top: -157px;
    width: 2105px;
    z-index: -1;
  }

  .img {
    height: 100%;
    left: 3.75%;
    object-fit: cover;
    position: absolute;
    top: 11.18%;
    width: 100%;
  }

  .welcome-screen {
    background-color: #001199;
    padding: 40px;
    border-radius: 20px;
    border: 4px solid #00d8ff;
    width: 100vw;
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: flex-start;
    box-sizing: border-box;
  }

  .cart-items {
    display: flex;
    flex-direction: column;
    gap: 10px;
    width: 100%;
  }
</style>
