<template>
  <div class="order-details-screen view-container">
    <div class="container max-w-full mx-auto px-8 pt-32">
      <div class="welcome-screen">
        <div v-if="loading" class="loading">Загрузка...</div>
        <div v-else-if="error" class="error">{{ error }}</div>
        <template v-else-if="order">
          <BigTitle :text="`Заказ #${order.id.slice(0,8)}`" class="max-w-1662 mx-auto" />
          <OrderProperties class="mt-20 max-w-1662 mx-auto" :status-lbl-text="'Статус:'" :status-text="order.status" :date-lbl-text="'Дата:'" :date-text="new Date(order.orderDate).toLocaleDateString()" :price-text="order.totalPrice + ' ₽'" />
          <div class="order-items mt-20 max-w-1662 mx-auto">
            <OrderItemKey v-for="item in displayedItems" :key="item.id" :game-text="item.gameTitle!" :seller-text="item.sellerName!" :key-text="revealedKeys.has(item.id) ? (item.key || 'Loading...') : '****-****-****-****'" @show-key="toggleKey(item.id)" />
          </div>
          <div class="flex justify-center mt-32 max-w-1662 mx-auto">
            <Pages :current-page="page" :total-pages="totalPages" @update:page="page = $event" />
          </div>
        </template>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
  import { computed } from 'vue';
  import BigTitle from '@/components/ui/titles/BigTitle.vue';
  import OrderProperties from '@/components/ui/orders/OrderProperties.vue';
  import OrderItemKey from '@/components/ui/orders/OrderItemKey.vue';
  import Pages from '@/components/ui/pages/Pages.vue';
  import { useOrderDetails } from '@/composables/useOrderDetails';
  import { usePagination } from '@/composables/usePagination';
  import type { OrderItem } from '@/types/order';
  const { order, revealedKeys, toggleKey, loading, error } = useOrderDetails();
  const { page, totalPages, displayedItems } = usePagination<OrderItem>(computed(() => order.value?.items || []));
</script>

<style scoped>
  .order-details-screen {
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

  .order-items {
    display: flex;
    flex-direction: column;
    gap: 10px;
    width: 100%;
  }
</style>
