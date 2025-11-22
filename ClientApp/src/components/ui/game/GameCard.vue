<template>
  <div class="game-card p-8 bg-black/30 rounded-xl border-2 border-transparent hover:border-[#00D9FF] transition cursor-pointer">
    <div class="game-available flex items-center gap-8">
      <GamePageBtn :game-page-btn-text="game.title" />
      <GamePriceLbl :game-price-lbl-text="game.price + ' ₽'" />
      <CanBuyBtn v-if="game.isForSale" />
      <CannotBuyBtn v-else />
      <MoreAboutGameBtn @click="$router.push('/games/' + game.id)" />
    </div>
  </div>
</template>

<script setup lang="ts">
  defineProps<{
    game: any;
  }>();
</script>
