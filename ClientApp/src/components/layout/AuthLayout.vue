<template>
  <div class="min-h-screen relative">
    <BG />
    <div class="flex items-center justify-center min-h-screen px-4">
      <slot />
    </div>
  </div>
</template>

<script setup lang="ts">
  import BG from '@/components/ui/BG.vue'
</script>

<style scoped>
  .auth-layout {
    position: relative;
    min-height: 100vh;
  }

  .bg {
    position: absolute;
    z-index: -1;
  }
</style>
