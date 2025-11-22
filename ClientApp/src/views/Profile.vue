<template>
  <div class="profile view-container">
    <div class="welcome-screen">
      <div class="border" />
      <BigTitle class="big-title" :text="profile.username ? auth.role : 'Профиль'" />
      <div class="info-layout">
        <InputExample class="input-field"
                      inputLblText="Имя:"
                      v-model="profile.username"
                      :readonly="true"
                      layoutClassName="input-example-2" />
        <InputExample class="input-field"
                      inputLblText="Электронная почта:"
                      v-model="profile.email"
                      :readonly="true"
                      layoutClassName="input-example-2" />
        <InputExample class="input-field"
                      inputLblText="Страна:"
                      v-model="profile.country"
                      :readonly="true"
                      layoutClassName="input-example-2" />
        <InputExample class="input-field"
                      inputLblText="Баланс:"
                      v-model="formattedBalance"
                      :readonly="true"
                      layoutClassName="input-example-2" />
        <InputExample class="input-field"
                      inputLblText="Общая сумма покупок:"
                      v-model="formattedTotalSpent"
                      :readonly="true"
                      layoutClassName="input-example-2" />
        <div class="replenish-layout flex items-center gap-4 mt-8">
          <InputExample inputLblText="Пополнить на:" v-model="amountString" placeholder="Сумма (₽)" type="number" min="0" class="w-48" />
          <div class="replenish-button" @click="replenish">Пополнить</div>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
  import BigTitle from '@/components/ui/titles/BigTitle.vue';
  import InputExample from '@/components/ui/InputExample.vue';
  import { useAuthStore } from '@/stores/auth';
  import { useProfile } from '@/composables/useProfile';
  const auth = useAuthStore();
  const { profile, amountString, formattedBalance, formattedTotalSpent, replenish } = useProfile();
</script>

<style scoped>
  .profile {
    width: 100vw;
    min-height: 100vh;
    display: flex;
    justify-content: center;
    align-items: center;
    background-color: #ffffff;
    padding: 0;
    margin: 0;
    position: relative;
    overflow: hidden;
  }

  .welcome-screen {
    background-color: #001199;
    padding: 40px;
    border-radius: 20px;
    border: 4px solid #00d8ff;
    width: 100vw;
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: flex-start;
    box-sizing: border-box;
  }

  .border {
    background-color: #001199;
    border: 6px solid #00d8ff;
    border-radius: 40px;
    height: 100%;
    width: 100%;
    position: absolute;
    top: 0;
    left: 0;
  }

  .big-title {
    color: #ffffff;
    font-size: 2.5rem;
    margin-bottom: 30px;
    text-align: center;
    position: relative;
    z-index: 1;
  }

  .info-layout {
    display: flex;
    flex-direction: column;
    gap: 20px;
    width: 100%;
    padding: 0 20px;
    box-sizing: border-box;
    position: relative;
    z-index: 1;
  }

  .input-field {
    width: 100%;
  }

  @media (min-width: 768px) {
    .info-layout {
      max-width: 1400px;
    }
  }

  .replenish-button {
    background-color: var(--color-gift-accent); /* #DA00A1 */
    border: var(--border-interactive) solid var(--color-pink-border); /* #890065 */
    border-radius: var(--radius-main);
    padding: 12px 24px;
    color: var(--color-text-primary);
    font-family: var(--font-main);
    font-size: var(--font-size-medium);
    cursor: pointer;
    text-align: center;
    white-space: nowrap;
  }

    .replenish-button:hover {
      box-shadow: 0 0 20px var(--color-border-1);
    }
</style>
