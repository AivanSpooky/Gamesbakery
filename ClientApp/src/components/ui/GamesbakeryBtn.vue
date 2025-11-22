<template>
  <div class="gamesbakery-btn cursor-pointer" @click="$router.push('/')">
    <div class="text-wrapper">Gamesbakery</div>
  </div>
</template>

<script setup lang="ts">
</script>

<style scoped>
  .gamesbakery-btn {
    background-color: var(--color-background-accent);
    border: 12px solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 120px;
    width: 507px;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-main);
    font-size: var(--font-size-large);
    font-weight: 400;
    text-align: center;
    line-height: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    height: 100%;
  }
</style>
