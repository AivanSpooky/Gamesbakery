<template>
  <button :class="['reg-lbl', className]" @click="$emit('click')">
    <div class="text-wrapper">{{ text }}</div>
  </button>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';

  export default defineComponent({
    name: 'RegLbl',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: 'Зарегистрироваться' },
    },
    emits: ['click'],
  });
</script>

<style scoped>
  .reg-lbl {
    background-color: #ff00f6; /* From Figma */
    border: 12px solid #da00a1; /* From Figma */
    border-radius: 40px; /* var(--radius-main) */
    height: 85px;
    position: relative;
    width: 100%;
    cursor: pointer;
  }

  .text-wrapper {
    color: #ffffff;
    font-family: "Roboto-Regular", Helvetica;
    font-size: 32px;
    font-weight: 400;
    height: 44.71%;
    left: 30.07%;
    letter-spacing: 0;
    line-height: normal;
    position: absolute;
    text-align: center;
    top: 28.24%;
    white-space: nowrap;
    width: 39.86%;
  }
</style>
