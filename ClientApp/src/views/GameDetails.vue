<template>
  <div class="game-details view-container">
    <div class="container max-w-full mx-auto px-8 pt-32">
      <div class="welcome-screen">
        <div v-if="loading" class="loading">Загрузка...</div>
        <div v-else-if="error" class="error">{{ error }}</div>
        <GameProperties v-else-if="game" class="mt-20 max-w-1662 mx-auto" :game="game" />
        <div class="order-items mt-32 max-w-1662 mx-auto">
          <OrderItem v-for="item in displayedItems" :key="item.id" :seller="item.sellerName" :rating="(item.averageRating ?? 0).toFixed(2)" :good="(item.averageRating ?? 0) >= 4" :id="item.id" />
        </div>
        <div class="flex justify-center mt-32 max-w-1662 mx-auto">
          <Pages :current-page="page" :total-pages="totalPages" @update:page="page = $event" />
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
  import GameProperties from '@/components/ui/game/GameProperties.vue';
  import OrderItem from '@/components/ui/orders/OrderItem.vue';
  import Pages from '@/components/ui/pages/Pages.vue';
  import { useGameDetails } from '@/composables/useGameDetails';
  import { usePagination } from '@/composables/usePagination';
  import type { GameOrderItem } from '@/types/game';
  const { game, orderItems, loading, error } = useGameDetails();
  const { page, totalPages, displayedItems } = usePagination<GameOrderItem>(orderItems, 1);
</script>

<style scoped>
  .game-details {
    position: relative;
    min-height: 100vh;
    overflow: hidden;
    background-color: var(--color-background);
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
    align-items: flex-start;
    background-color: #001199;
    display: flex;
    flex-direction: column;
    gap: 10px;
    overflow: hidden;
    padding: 10px;
  }

  .max-w-1662 {
    max-width: 1662px;
  }

  .mx-auto {
    margin-left: auto;
    margin-right: auto;
  }
</style>
