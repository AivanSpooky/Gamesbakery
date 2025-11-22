<template>
  <div :class="['description', className]">
    <div class="desc-title-lbl">
      <div class="text-wrapper">{{ title }}</div>
    </div>
    <div class="desc-lbl">
      <p v-if="readonly" class="p-text">{{ text }}</p>
      <textarea v-else v-model="model" :placeholder="placeholder" class="p-text" />
    </div>
  </div>
</template>
<script lang="ts">
  import { defineComponent, ref, watch } from 'vue';
  export default defineComponent({
    name: 'Description',
    props: {
      className: { type: String, default: '' },
      title: { type: String, default: 'Описание' },
      text: { type: String, default: '' },
      placeholder: { type: String, default: 'Тут текст отзыва...' },
      readonly: { type: Boolean, default: true },
    },
    setup(props, { emit }) {
      const model = ref(props.text);
      watch(model, (val) => emit('update:modelValue', val));
      return { model };
    },
  });
</script>
<style scoped>
  .description {
    display: flex;
    flex-direction: column;
    gap: 0;
    height: 179px;
    overflow: hidden;
    position: relative;
    width: 100%;
    max-width: 1640px;
  }

  .desc-title-lbl {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 79px;
    overflow: hidden;
    width: 100%;
    max-width: 507px;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-medium);
    font-weight: 400;
    text-align: center;
    white-space: nowrap;
  }

  .desc-lbl {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    padding: 23px 39px;
    width: 100%;
    overflow: hidden;
    margin-top: -12px;
  }

  .p-text {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-small);
    font-weight: 400;
    line-height: normal;
    white-space: normal;
    background: transparent;
    border: none;
    resize: none;
    width: 100%;
    height: 100%;
  }

    .p-text:focus {
      outline: none;
    }
</style>
