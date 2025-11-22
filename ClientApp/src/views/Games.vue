<template>
  <div class="games-screen view-container">
    <div class="container max-w-full mx-auto px-8 pt-32">
      <div class="welcome-screen">
        <GameFilters class="mt-20 max-w-1662 mx-auto" @filter="applyFilters" />
        <div class="view max-w-1662 mx-auto">
          <GameItem v-for="game in displayedItems" :key="game.id" :available="game.isForSale" :gamePageBtnText="game.title" :gamePriceLblText="game.price.toString()" :id="game.id" />
        </div>
        <div class="flex justify-center mt-32 max-w-1662 mx-auto">
          <Pages :current-page="page" :total-pages="totalPages" @update:page="page = $event" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import GameFilters from '@/components/ui/filters/GameFilters.vue';
  import GameItem from '@/components/ui/game/GameItem.vue';
  import Pages from '@/components/ui/pages/Pages.vue';
  import { useGamesList } from '@/composables/useGame';
  import { usePagination } from '@/composables/usePagination';  // Импортируем ваш composable
  import type { Game } from '@/types/game';

  const { games, applyFilters } = useGamesList();
  const { page, totalPages, displayedItems } = usePagination<Game>(games, 10);  // Клиентская пагинация по 10 на страницу
</script>

<style scoped>
  .games-screen {
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
    background-color: var(--color-background);
    padding: 40px;
    border-radius: 20px;
    border: 4px solid var(--color-background-border);
    width: 100vw;
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: flex-start;
    box-sizing: border-box;
  }

  .view {
    width: 65%;
    background-color: var(--color-background);
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    position: relative;
    gap: 20px;
  }
</style>
