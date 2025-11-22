<template>
  <div :class="['cannot-buy-btn', className]">
    <Dell />
  </div>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';
  import Dell from '@/components/icons/Dell.vue';

  export default defineComponent({
    name: 'CannotBuyBtn',
    components: { Dell },
    props: { className: { type: String, default: '' } },
  });
</script>

<style scoped>
  .cannot-buy-btn {
    background-color: var(--color-btn-accent);
    border: var(--border-interactive) solid var(--color-border-3);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 90px;
    width: 113px;
    overflow: hidden;
  }
</style>
