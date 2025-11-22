<template>
  <div :class="['to-price', className]">
    <input class="text-wrapper neon-input" type="number" v-model="value" :placeholder="placeholder" maxlength="5" @input="validateInput" />
  </div>
</template>
<script lang="ts">
  import { defineComponent, ref, watch } from 'vue';
  export default defineComponent({
    name: 'ToPrice',
    props: {
      className: { type: String, default: '' },
      placeholder: { type: String, default: '299' },
      modelValue: { type: [String, Number], default: '' },
    },
    emits: ['update:modelValue'],
    setup(props, { emit }) {
      const value = ref(props.modelValue ? String(props.modelValue) : '');
      const validateInput = (event: Event) => {
        const input = event.target as HTMLInputElement;
        input.value = input.value.replace(/\D/g, '').slice(0, 5);
        value.value = input.value;
        emit('update:modelValue', value.value);
      };
      watch(() => props.modelValue, (newVal) => {
        value.value = newVal ? String(newVal) : '';
      });
      return { value, validateInput };
    },
  });
</script>
<style scoped>
  .to-price {
    background-color: var(--color-money-accent);
    border: 6px solid var(--color-yellow-border);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 90px;
  }

  .text-wrapper {
    color: var(--color-text-secondary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    font-weight: 400;
    text-align: center;
    background: transparent;
    border: none;
    width: 150px;
  }
</style>
