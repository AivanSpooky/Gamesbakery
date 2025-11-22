<template>
  <div class="cart-btn cursor-pointer" @click="$router.push('/cart/index')">
    <BasketAlt3 class="basket-alt" />
  </div>
</template>

<script setup lang="ts">
  import BasketAlt3 from '@/components/icons/BasketAlt3.vue';
</script>

<style scoped>
  .cart-btn {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 120px;
    width: 115px;
    overflow: hidden;
    cursor: pointer;
  }

  .basket-alt {
    height: 72px;
    width: 72px;
  }
</style>
