<template>
  <div class="register-btn cursor-pointer" @click="navigateToRegister">
    <div class="text-wrapper">Регистрация</div>
  </div>
</template>
<script setup lang="ts">
  import { useRouter } from 'vue-router'; // Added import
  const router = useRouter(); // Use useRouter
  const navigateToRegister = () => {
    router.push('/account/register');
  };
</script>

<style scoped>
  .register-btn {
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

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    font-weight: 400;
    text-align: center;
  }
</style>
