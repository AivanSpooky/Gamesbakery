<template>
  <button :class="['have-acc-lbl', className]" @click="$emit('click')">
    <div class="text-wrapper">{{ text }}</div>
  </button>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';

  export default defineComponent({
    name: 'HaveAccLbl',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: 'У меня уже есть аккаунт' },
    },
    emits: ['click'],
  });
</script>

<style scoped>
  .have-acc-lbl {
    background-color: #13baba; /* From Figma */
    border: 12px solid #0a6262; /* From Figma */
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
    left: 24.76%;
    letter-spacing: 0;
    line-height: normal;
    position: absolute;
    text-align: center;
    top: 28.24%;
    white-space: nowrap;
    width: 50.34%;
  }
</style>
