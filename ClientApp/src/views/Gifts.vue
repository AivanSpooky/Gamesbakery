<template>
  <div class="gifts-screen view-container">
    <div class="container max-w-full mx-auto px-8 pt-32">
      <div class="welcome-screen">
        <BigTitle text="Подарки" class="max-w-1662 mx-auto" />
        <GiftProperties class="mt-20 max-w-1662 mx-auto" title="Подарки" type-lbl-text="Тип:" @filter="applyFilter" />
        <div class="gifts-list mt-20 max-w-1662 mx-auto">
          <GiftExample v-for="gift in displayedGifts" :key="gift.id" :from="gift.from" :type="gift.type" :status="gift.status" />
        </div>
        <TableSellerLbl text="Создать подарок" class="create-gift-btn mt-20 max-w-1662 mx-auto cursor-pointer" @click="router.push('/gift/create')" />
        <div class="flex justify-center mt-32 max-w-1662 mx-auto">
          <Pages :current-page="page" :total-pages="totalPages" @update:page="page = $event" />
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
  import { useRouter } from 'vue-router';
  import { useGiftsList } from '@/composables/useGiftsList';
  import { usePagination } from '@/composables/usePagination';
  import BigTitle from '@/components/ui/titles/BigTitle.vue';
  import GiftProperties from '@/components/ui/gifts/GiftProperties.vue';
  import GiftExample from '@/components/ui/gifts/GiftExample.vue';
  import TableSellerLbl from '@/components/ui/table/TableSellerLbl.vue';
  import Pages from '@/components/ui/pages/Pages.vue';
  import type { Gift } from '@/types/gift';
  const router = useRouter();
  const { gifts, applyFilter } = useGiftsList();
  const { page, totalPages, displayedItems: displayedGifts } = usePagination<Gift>(gifts);
</script>

<style scoped>
  .gifts-screen {
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

  .gifts-list {
    display: flex;
    flex-direction: column;
    gap: 10px;
    width: 100%;
  }

  .create-gift-btn {
    width: 494px;
    align-self: flex-end;
  }
</style>
