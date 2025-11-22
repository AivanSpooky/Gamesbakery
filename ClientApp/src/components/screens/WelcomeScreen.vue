<template>
  <div class="welcome-screen">
    <div class="div">
      <!-- Inner border from Figma -->
      <BigTitle class="big-title-instance" text="Добро пожаловать!" />
      <div class="rectangle-2">
        <p class="p">
          Здесь будет приветственный текст. Далее просто нажимай на кнопки сверху!
        </p>
        <!-- Additional content or BG can go here -->
      </div>
    </div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import BigTitle from '@/components/ui/titles/BigTitle.vue';

  export default defineComponent({
    name: 'WelcomeScreen',
    components: { BigTitle }
  });
</script>
<style scoped>
  .welcome-screen {
    background-color: var(--color-background); /* #001299 */
    position: relative;
    width: 100%;
    max-width: 1662px; /* From Figma */
    min-height: 887px; /* From Figma, but min- for responsiveness */
    margin: 0 auto; /* Center it */
  }

  .div {
    background-color: var(--color-background); /* #001299 */
    border: 6px solid var(--color-border-1); /* #00d8ff */
    border-radius: var(--radius-main); /* 40px */
    position: relative; /* Changed from absolute for better flow */
    width: 100%;
    max-width: 1640px; /* From Figma */
    min-height: 847px; /* From Figma */
    margin: 20px auto 0; /* Padding equivalent to Figma's top:20px */
    padding: 20px; /* Add internal padding for content spacing */
  }

  .p {
    color: var(--color-text-primary); /* #ffffff */
    font-family: var(--font-additional), sans-serif; /* Roboto with fallback */
    font-size: var(--font-size-main); /* 32px */
    font-weight: 400;
    letter-spacing: 0;
    line-height: normal;
    position: relative; /* Relative for stacking */
    width: 100%;
    max-width: 1498px; /* From Figma */
    margin: 20px auto 0; /* Position inside rectangle */
    padding: 0 24px; /* Left padding from Figma left:82px relative to inner */
  }

  .rectangle-2 {
    background-color: var(--color-background-accent); /* #067aff */
    border: 12px solid var(--color-border-1); /* #00d8ff */
    border-radius: var(--radius-main); /* 40px */
    position: relative; /* Relative */
    width: 100%;
    max-width: 1546px; /* From Figma */
    min-height: 618px; /* From Figma */
    margin: 20px auto 0; /* Position below BigTitle */
    padding: 20px; /* Padding inside the frame for text */
  }

  .big-title-instance {
    position: relative; /* Relative */
    width: 100%;
    margin: 48px auto 0; /* Approximate top:68px adjusted for padding */
  }
</style>
