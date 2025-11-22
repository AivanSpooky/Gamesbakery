<template>
  <header class="fixed top-0 left-0 w-full z-50 bg-[var(--color-background)] border-b-[var(--border-interactive)] border-solid border-[var(--color-border-1)]">
    <nav class="max-w-[1920px] mx-auto flex flex-row flex-nowrap items-center h-[160px] px-[20px] overflow-x-auto">
      <!-- Левая часть -->
      <div class="other-btns flex flex-1 items-center gap-[17px] flex-nowrap shrink-0">
        <GamesbakeryBtn />
        <GamesBtn />
        <OrdersBtn v-if="auth.isAuthenticated" />
        <GiftsBtn v-if="auth.isAuthenticated && auth.role === 'User'" />
        <OrderItemsBtn v-if="auth.isAuthenticated && ['Admin', 'Seller'].includes(auth.role)" />
        <UsersBtn v-if="auth.isAuthenticated && auth.role === 'Admin'" />
        <template v-if="auth.isAuthenticated">
          <CartBtn v-if="auth.role === 'User'" />
          <ProfileBtn />
          <LogOutBtn @click="logout" />
        </template>
        <template v-else>
          <LogInBtn />
          <RegisterBtn />
        </template>
      </div>
    </nav>
  </header>
</template>
<script setup lang="ts">
  import { useAuthStore } from '@/stores/auth';
  import { AuthService } from '@/services/AuthService';
  import { useRouter } from 'vue-router';
  import GamesbakeryBtn from '@/components/ui/GamesbakeryBtn.vue';
  import GamesBtn from '@/components/ui/GamesBtn.vue';
  import OrdersBtn from '@/components/ui/OrdersBtn.vue';
  import GiftsBtn from '@/components/ui/GiftsBtn.vue';
  import CartBtn from '@/components/ui/CartBtn.vue';
  import ProfileBtn from '@/components/ui/ProfileBtn.vue';
  import LogOutBtn from '@/components/ui/LogOutBtn.vue';
  import LogInBtn from '@/components/ui/LogInBtn.vue';
  import RegisterBtn from '@/components/ui/RegisterBtn.vue';
  import OrderItemsBtn from '@/components/ui/OrderItemsBtn.vue';
  import UsersBtn from '@/components/ui/UsersBtn.vue';
  const auth = useAuthStore();
  const router = useRouter();
  const logout = () => {
    AuthService.logout();
    router.push('/');
  };
</script>
<style scoped>
  .other-btns {
    display: flex;
    align-items: center;
    gap: 17px;
    flex-wrap: nowrap;
  }

  .auth-btns {
    display: flex;
    align-items: center;
    gap: 11px;
    flex-wrap: nowrap;
  }

  .header {
    backdrop-filter: blur(10px);
  }

  .logo {
    width: 120px;
  }
</style>
