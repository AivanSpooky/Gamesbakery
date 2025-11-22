<template>
  <div :class="['category', className]">
    <input class="text-wrapper neon-input" v-model="value" :placeholder="placeholder" :readonly="readonly" />
  </div>
</template>
<script lang="ts">
  import { defineComponent, ref } from 'vue';
  export default defineComponent({
    name: 'Category',
    props: {
      className: { type: String, default: '' },
      placeholder: { type: String, default: 'Стратегии' },
      readonly: { type: Boolean, default: false },
    },
    setup(props) {
      const value = ref(props.placeholder);
      return { value };
    },
  });
</script>
<style scoped>
  .category {
    background-color: var(--color-teal-accent);
    border: var(--border-notification) solid var(--color-teal-border);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 90px;
    width: 305px;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-main);
    font-size: var(--font-size-main);
    font-weight: 400;
    text-align: center;
    background: transparent;
    border: none;
    width: 328px;
  }

    .text-wrapper:read-only {
      cursor: default;
      opacity: 0.8;
    }
</style>
