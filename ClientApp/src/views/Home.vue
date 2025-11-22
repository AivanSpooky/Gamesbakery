<template>
  <div class="home view-container">
    <BG class="bg" />
    <WelcomeScreen />
  </div>
</template>
<script lang="ts">
  import { defineComponent, onMounted } from 'vue';
  import BG from '@/components/ui/BG.vue';
  import Header from '@/components/layout/Header.vue';
  import WelcomeScreen from '@/components/screens/WelcomeScreen.vue';
  import { GameService } from '@/services/GameService'; // Example: Load featured games
  export default defineComponent({
    name: 'Home',
    components: { BG, Header, WelcomeScreen },
    setup() {
      onMounted(async () => {
        // Example business logic: Load home data
        await GameService.getAll(); // If needed for home
      });
    },
  });
</script>

<style scoped>
  .home {
    position: relative;
    min-height: 100vh;
    overflow: hidden;
    background-color: var(--color-background);
    display: flex;
    flex-direction: column;
    align-items: center;
  }

  .bg {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    object-fit: cover;
    z-index: -1;
  }
</style>
