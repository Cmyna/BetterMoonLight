import { useValue, trigger, bindValue } from "cs2/api";
import mod from 'mod.json'


export const useBinding = <T>(name: string): [T, (v: T) => void] => {
    const binding = bindValue<T>(mod.id, name)
    return [
        useValue<T>(binding), 
        (v) => trigger(mod.id, "Set" + name, v)
    ]
}


export const useTrigger = <T>(name: string): (v: T) => void => {
    return (v) => trigger(mod.id, "Set" + name, v)
}
