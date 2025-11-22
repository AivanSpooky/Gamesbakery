<template>
  <div :class="['put-to-cart-btn', className]" :disabled="disabled" @click="!disabled && $emit('add-to-cart')">
    <BasketAlt3 class="basket-alt" />
    <span v-if="disabled">Уже в корзине</span>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import BasketAlt3 from '@/components/icons/BasketAlt3.vue';
  export default defineComponent({
    name: 'PutToCartBtn',
    components: { BasketAlt3 },
    props: {
      className: { type: String, default: '' },
      disabled: { type: Boolean, default: false },
    },
    emits: ['add-to-cart'],
  });
</script>
<style scoped>
  .put-to-cart-btn {
    background-color: var(--color-money-accent);
    border: 12px solid var(--color-yellow-border);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 90px;
    width: 154px;
    overflow: hidden;
    cursor: pointer;
  }

  .basket-alt {
    width: 72px;
    height: 72px;
  }
</style>
