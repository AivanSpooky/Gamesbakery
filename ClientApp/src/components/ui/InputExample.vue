<template>
  <div :class="['input-example', className]">
    <div :class="['layout', layoutClassName]">
      <InputLbl :className="inputLblDivClassName" :text="inputLblText" />
      <Input :className="inputDivClassName" :placeholder="placeholder" :type="type" :readonly="readonly" v-model="model" />
    </div>
  </div>
</template>

<script setup lang="ts">
  import { defineProps, defineModel, withDefaults } from 'vue';
  import InputLbl from './InputLbl.vue';
  import Input from './Input.vue';

  interface Props {
    className?: string;
    layoutClassName?: string;
    inputLblDivClassName?: string;
    inputDivClassName?: string;
    inputLblText?: string;
    placeholder?: string;
    type?: string;
    readonly?: boolean;
  }

  const props = withDefaults(defineProps<Props>(), {
    className: '',
    layoutClassName: '',
    inputLblDivClassName: '',
    inputDivClassName: '',
    inputLblText: 'Label:',
    placeholder: 'Введите значение',
    type: 'text',
    readonly: false,
  });

  const model = defineModel<string>();
</script>

<style scoped>
  .input-example {
    background-color: var(--color-background);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    height: 121px;
    overflow: visible; /* Изменено на visible, чтобы избежать скрытия */
    position: relative;
    width: 100%;
    visibility: visible;
  }

  .layout {
    height: 100%;
    position: relative;
    width: 100%;
  }
</style>
