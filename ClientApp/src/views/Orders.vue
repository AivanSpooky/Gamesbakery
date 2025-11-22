<template>
  <div class="orders-screen view-container">
    <div class="container max-w-full mx-auto px-8 pt-32">
      <div class="welcome-screen">
        <BigTitle :text="isSeller ? 'Мои ключи' : 'Заказы'" class="max-w-1662 mx-auto" />
        <div v-if="loading" class="loading">Загрузка...</div>
        <div v-else-if="error" class="error">{{ error }}</div>
        <div v-else-if="!items.length" class="no-orders">Нет элементов</div>
        <div v-else class="orders-list mt-20 max-w-1662 mx-auto">
          <template v-if="!isSeller">
            <OrderRow v-for="item in displayedItems" :key="item.id" :order-id="item.id" :total-price="item.totalAmount!" :date="new Date(item.orderDate!).toLocaleDateString()" :status="item.status!" :to-details="`/order/details/${item.id}`" />
          </template>
          <template v-else>
            <OrderItemsSeller v-for="item in displayedItems" :key="item.id" :game-text="item.gameTitle!" :price-text="item.gamePrice!" :status-text="item.status!" />
          </template>
        </div>
        <div class="flex justify-center mt-32 max-w-1662 mx-auto">
          <Pages :current-page="page" :total-pages="totalPages" @update:page="page = $event" />
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
  import BigTitle from '@/components/ui/titles/BigTitle.vue';
  import OrderRow from '@/components/ui/orders/OrderRow.vue';
  import OrderItemsSeller from '@/components/ui/orders/OrderItemsSeller.vue';
  import Pages from '@/components/ui/pages/Pages.vue';
  import { useOrdersList } from '@/composables/useOrdersList';
  import { usePagination } from '@/composables/usePagination';
  import type { OrderItem } from '@/types/order';
  const { isSeller, items, loading, error } = useOrdersList();
  const { page, totalPages, displayedItems } = usePagination<OrderItem>(items);
</script>

<style scoped>
  .orders-screen {
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

  .orders-list {
    display: flex;
    flex-direction: column;
    gap: 10px;
    width: 100%;
  }

  .loading, .error, .no-orders {
    color: var(--color-text-primary);
    font-size: var(--font-size-main);
    margin-top: 20px;
  }
</style>
