<template>
  <div class="create-gift-screen view-container">
    <div class="container max-w-full mx-auto px-8 pt-32">
      <div class="welcome-screen">
        <BigTitle text="Создание подарка" class="max-w-1662 mx-auto" />
        <div class="input-layout mt-20 max-w-1662 mx-auto">
          <InputExample inputLblText="Кому (ID):" v-model="recipientId" placeholder="ID получателя" />
          <div class="gift-key-layout">
            <div class="gift-lay flex flex-col gap-4">
              <GiftKeyExample v-for="(item, i) in items" :key="i" :game-title="item.gameTitle" @remove="items.splice(i, 1)" />
              <GiftKeyAddExample @add="addItem" />
            </div>
          </div>
          <ActionsExample regLblText="Отправить подарок" haveAccLblText="Отмена" @regClick="sendGift" @haveAccClick="cancel" />
        </div>
      </div>
    </div>
    <div v-if="showModal" class="modal-overlay">
      <div class="modal-box">
        <h3>Выберите ключ для подарка</h3>
        <select v-model="selectedOrderItemId">
          <option value="">Выберите ключ</option>
          <option v-for="item in available" :key="item.id" :value="item.id">{{ item.gameTitle }}</option>
        </select>
        <div class="modal-actions">
          <button @click="addSelected">Добавить</button>
          <button @click="closeModal">Отмена</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import BigTitle from '@/components/ui/titles/BigTitle.vue';
  import InputExample from '@/components/ui/InputExample.vue';
  import GiftKeyExample from '@/components/ui/gifts/GiftKeyExample.vue';
  import GiftKeyAddExample from '@/components/ui/gifts/GiftKeyAddExample.vue';
  import ActionsExample from '@/components/ui/ActionsExample.vue';
  import { useGiftCreate } from '@/composables/useGiftCreate';

  const { recipientId, items, showModal, selectedOrderItemId, available, addItem, addSelected, closeModal, sendGift, cancel } = useGiftCreate();
</script>

<style scoped>
  /* Existing styles remain unchanged, with additions for modal */
  .create-gift-screen {
    position: relative;
    height: 100vh;
    overflow: auto;
    background-color: var(--color-background);
    display: flex;
    justify-content: center;
    align-items: flex-start;
  }

  .bg {
    aspect-ratio: 1.5;
    height: 1404px;
    left: -79px;
    position: absolute;
    top: -157px;
    width: 2105px;
    z-index: -1;
  }

  .img {
    height: 100%;
    left: 3.75%;
    object-fit: cover;
    position: absolute;
    top: 11.18%;
    width: 100%;
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

  .input-layout {
    display: flex;
    flex-direction: column;
    gap: 10px;
    width: 100%;
  }

  .gift-key-layout {
    background-color: #001199;
    border: 12px solid #00d8ff;
    border-radius: 40px;
    overflow: hidden;
    position: relative;
    width: 100%;
    padding: 10px;
  }

  .gift-lay {
    align-items: center;
    display: flex;
    flex-direction: column;
    gap: 10px;
    position: relative;
  }
  /* Modal styles */
  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
  }

  .modal-box {
    background-color: #001199;
    border: 4px solid #00d8ff;
    padding: 20px;
    border-radius: 20px;
    width: 80%;
    max-width: 500px;
    color: white;
  }

    .modal-box h3 {
      font-size: 1.5rem;
      margin-bottom: 10px;
    }

    .modal-box select {
      width: 100%;
      padding: 10px;
      margin-bottom: 10px;
      border-radius: 10px;
      background-color: #00d8ff;
      color: black;
    }

  .modal-actions {
    display: flex;
    gap: 10px;
    justify-content: flex-end;
  }

    .modal-actions button {
      padding: 10px 20px;
      border-radius: 10px;
      border: none;
      cursor: pointer;
    }

      .modal-actions button:first-child {
        background-color: #ff00ff;
        color: white;
      }

      .modal-actions button:last-child {
        background-color: #00ffff;
        color: black;
      }
</style>
