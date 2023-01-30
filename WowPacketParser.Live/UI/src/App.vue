<script lang="ts" setup>
import {Ref, ref} from "vue";
import {Packet} from "./types";
import Header from "./components/Header.vue";
import PacketList from "./components/PacketList.vue";
import PacketDetails from "./components/PacketDetails.vue";
import PacketSummary from "./components/PacketSummary.vue";

const packets: Ref<Packet[]> = ref([])
const selected: Ref<Packet | undefined> = ref()

function handlePacket(packet: Packet) {
    packets.value.push(packet)
}

</script>
<template>
    <Header
        @clear="packets = []"
        @packet=handlePacket
    />

    <PacketList
        v-model:selected="selected"
        :packets="packets"
    />
    <template v-if="!!selected">
        <PacketSummary

            :packet="selected"
            active/>
        <PacketDetails :packet="selected"/>
    </template>
</template>