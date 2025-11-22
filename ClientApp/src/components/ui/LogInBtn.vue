<template>
  <div class="log-in-btn cursor-pointer" @click="$router.push('/account/login')">
    <div class="labels">
      <div class="text-wrapper">Войти</div>
      <SignInSquare class="sign-in-square" />
    </div>
  </div>
</template>

<script setup lang="ts">
  import SignInSquare from '@/components/icons/SignInSquare.vue';
</script>

<style scoped>
  .log-in-btn {
    background-color: var(--color-background-accent);
    border: 12px solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 120px;
    width: 230px;
    overflow: hidden;
  }

  .labels {
    display: flex;
    align-items: center;
    gap: 11px;
    padding: 0 1px;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    font-weight: 400;
    text-align: center;
  }
</style>
