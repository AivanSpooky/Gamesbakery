<template>
  <div :class="['can-buy-btn', className]">
    <DoneRingRound />
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import DoneRingRound from '@/components/icons/DoneRingRound.vue';
  export default defineComponent({
    name: 'CanBuyBtn',
    components: { DoneRingRound },
    props: { className: { type: String, default: '' } },
  });
</script>
<style scoped>
  .can-buy-btn {
    background-color: var(--color-success);
    border: var(--border-interactive) solid var(--color-border-2);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 90px;
    width: 113px;
    overflow: hidden;
  }
</style>
