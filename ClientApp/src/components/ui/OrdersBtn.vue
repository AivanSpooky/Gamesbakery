<template>
  <div :class="['orders-btn', className]" @click="goToOrders">
    <div class="text-wrapper">Заказы</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import { useRouter } from 'vue-router';

  export default defineComponent({
    name: 'OrdersBtn',
    props: {
      className: { type: String, default: '' },
    },
    setup() {
      const router = useRouter();
      const goToOrders = () => {
        router.push('/order/index');
      };
      return { goToOrders };
    },
  });
</script>
<style scoped>
  .orders-btn {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 120px;
    width: 180px;
    overflow: hidden;
    cursor: pointer;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    font-weight: 400;
    text-align: center;
    white-space: nowrap;
  }
</style>
