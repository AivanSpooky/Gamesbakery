<template>
  <div :class="['more-about-game-btn', className]" @click="$emit('click')">
    <Info class="info-icon" />
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import Info from '@/components/icons/Info.vue';
  export default defineComponent({
    name: 'MoreAboutGameBtn',
    components: { Info },
    props: { className: { type: String, default: '' } },
    emits: ['click'],
  });
</script>
<style scoped>
  .more-about-game-btn {
    background-color: var(--color-teal-accent);
    border: 12px solid var(--color-teal-border);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 90px;
    width: 113px;
    overflow: visible;
    cursor: pointer;
  }

  .info-icon {
    width: 72px;
    height: 72px;
    margin-left: 8.5px;
    margin-top: -3px;
  }
</style>
