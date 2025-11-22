<template>
  <div class="games-btn cursor-pointer" @click="$router.push('/game/index')">
    <div class="text-wrapper">Игры</div>
  </div>
</template>

<script setup lang="ts">
</script>

<style scoped>
  .games-btn {
    background-color: var(--color-background-accent);
    border: 12px solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 120px;
    width: 180px;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    font-weight: 500;
    text-align: center;
  }
</style>
