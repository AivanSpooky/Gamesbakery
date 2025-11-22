<template>
  <div :class="['actions-example', className]">
    <div class="layout flex items-center justify-center gap-2 flex-nowrap">
      <RegLbl :text="regLblText" @click="$emit('regClick')" />
      <HaveAccLbl :text="haveAccLblText" @click="$emit('haveAccClick')" />
    </div>
  </div>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';
  import RegLbl from './RegLbl.vue';
  import HaveAccLbl from './HaveAccLbl.vue';

  export default defineComponent({
    name: 'ActionsExample',
    components: { RegLbl, HaveAccLbl },
    props: {
      className: { type: String, default: '' },
      regLblText: { type: String, default: 'Зарегистрироваться' },
      haveAccLblText: { type: String, default: 'У меня уже есть аккаунт' },
    },
    emits: ['regClick', 'haveAccClick'],
  });
</script>

<style scoped>
  .actions-example {
    background-color: #001199; /* From Figma */
    border: 12px solid #00d8ff; /* From Figma */
    border-radius: 40px; /* var(--radius-main) */
    display: flex;
    overflow: hidden;
    position: relative;
    width: 100%;
  }

  .layout {
    align-items: center;
    display: flex;
    gap: 10px;
    height: 121px;
    justify-content: center;
    padding: 18px 30px;
    position: relative;
    width: 100%;
    flex-wrap: nowrap; /* Ensure inline row */
  }
</style>
