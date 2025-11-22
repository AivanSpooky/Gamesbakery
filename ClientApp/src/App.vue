<template>
  <div class="min-h-screen relative">
    <BG />
    <Header />
    <main class="pt-[160px] px-8">
      <router-view />
    </main>
    <div class="notification-container">
      <transition-group name="notification" tag="div">
        <component :is="typeToComponent[n.type]"
                   v-for="n in notifications"
                   :key="n.id"
                   :message="n.message"
                   @close="store.remove(n.id)" />
      </transition-group>
    </div>
  </div>
</template>
<script setup lang="ts">
  import { onMounted, onUnmounted } from 'vue';
  import { storeToRefs } from 'pinia';
  import BG from '@/components/ui/BG.vue';
  import Header from '@/components/layout/Header.vue';
  import NotificationSuccess from '@/components/ui/notifications/NotificationSuccess.vue';
  import NotificationWarning from '@/components/ui/notifications/NotificationWarning.vue';
  import NotificationError from '@/components/ui/notifications/NotificationError.vue';
  import { useNotificationStore } from '@/stores/notification';

  const typeToComponent = {
    success: NotificationSuccess,
    warning: NotificationWarning,
    error: NotificationError,
  };

  const store = useNotificationStore();
  const { notifications } = storeToRefs(store);
</script>
<style scoped>
  .notification-container {
    position: fixed;
    top: 20px;
    right: 20px;
    z-index: 1000;
    display: flex;
    flex-direction: column;
    gap: 10px;
    align-items: flex-end;
  }

  .notification-enter-active,
  .notification-leave-active {
    transition: opacity 0.5s ease;
  }

  .notification-enter-from,
  .notification-leave-to {
    opacity: 0;
  }
  .app-container {
    position: relative;
    min-height: 100vh;
    overflow: hidden;
    background-color: var(--color-background);
    width: 100%;
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

  .header {
    width: 100%;
    z-index: 1;
  }

  .main-content {
    padding-top: var(--section-gap);
    z-index: 1;
  }
</style>
