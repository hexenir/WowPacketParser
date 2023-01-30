<script lang="ts" setup>
import type {Packet} from "../types";
import {computed} from "vue";

const props = defineProps<{
    packet: Packet
    active: boolean
}>()

const emit = defineEmits<{
    (e: "click"): void
}>()

const colorClass = computed(() => props.packet?.direction == "ClientToServer" ? "is-success" : "is-link")

const label = computed(() => `${props.packet?.name} (${'0x' + props.packet?.opCode.toString(16)})`)

const time = computed(() => format(new Date(props.packet?.timestamp * 1000 + (new Date()).getTimezoneOffset() * 60000), "HH:mm:ss"))


function format(date: Date, formatString: string): string {
    return Object.entries({
        YYYY: date.getFullYear(),
        YY: date.getFullYear().toString().substring(2),
        yyyy: date.getFullYear(),
        yy: date.getFullYear().toString().substring(2),
        MMMM: date.toLocaleString('default', {month: 'long'}),
        MMM: date.toLocaleString('default', {month: 'short'}),
        MM: (date.getMonth() + 1).toString().padStart(2, '0'),
        M: date.getMonth() + 1,
        DDDD: date.toLocaleDateString('default', {weekday: 'long'}),
        DDD: date.toLocaleDateString('default', {weekday: 'short'}),
        DD: date.getDate().toString().padStart(2, '0'),
        D: date.getDate(),
        dddd: date.toLocaleDateString('default', {weekday: 'long'}),
        ddd: date.toLocaleDateString('default', {weekday: 'short'}),
        dd: date.getDate().toString().padStart(2, '0'),
        d: date.getDate(),
        HH: date.getHours().toString().padStart(2, '0'), // military
        H: date.getHours().toString(), // military
        hh: (date.getHours() % 12).toString().padStart(2, '0'),
        h: (date.getHours() % 12).toString(),
        mm: date.getMinutes().toString().padStart(2, '0'),
        m: date.getMinutes(),
        SS: date.getSeconds().toString().padStart(2, '0'),
        S: date.getSeconds(),
        ss: date.getSeconds().toString().padStart(2, '0'),
        s: date.getSeconds(),
        TTT: date.getMilliseconds().toString().padStart(3, '0'),
        ttt: date.getMilliseconds().toString().padStart(3, '0'),
        AMPM: date.getHours() < 13 ? 'AM' : 'PM',
        ampm: date.getHours() < 13 ? 'am' : 'pm',
    }).reduce((acc, entry) => {
        return acc.replace(entry[0], entry[1].toString())
    }, formatString)
}

//$: time = format(date + date.getTimezoneOffset() , "HH:mm:ss")

</script>
<template>
    <div class="columns is-centered is-vcentered {bgClass}">
        <div
            class="column is-one-third is-vcentered is-flex is-justify-content-start"
            style="cursor: pointer"
            @click="emit('click')"
        >
            <div class="pr-3">
            <span
                class="tag {{colorClass}}">{{ packet.direction }}</span>
            </div>
            <div>Number: {{ packet.number }}</div>
        </div>
        <div class="column">{{ label }}</div>
        <div class="column is-one-third">Timestamp: {{ time }}</div>
    </div>
</template>