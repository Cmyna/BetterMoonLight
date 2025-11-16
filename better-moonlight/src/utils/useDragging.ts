import { useCallback, useEffect, useState } from "react"



type Pos2D = {
    readonly x: number,
    readonly y: number,
}


export const useMouseDrag = () => {
    type DragState = {
        offset: Pos2D,
        lastOffset: Pos2D,
        dragState?: {
            start: Pos2D,
            current: Pos2D
        }
    }

    const INITIAL_OFFSET = {x: 0, y: 0}

    const [dragState, setDragState] = useState<DragState>({
        offset: INITIAL_OFFSET,
        lastOffset: INITIAL_OFFSET
    });


    // const register listener event on windows
    useEffect(() => {
        const mouseMove = (e: MouseEvent) => onDragging({x: e.clientX, y: e.clientY});
        const mouseUp = () => onDragEnd();
        window.addEventListener("mousemove", mouseMove)
        window.addEventListener("mouseup", mouseUp);

        return () => {
            window.removeEventListener("mousemove", mouseMove)
            window.removeEventListener("mouseup", mouseUp)
        }
    }, [])


    const resetOffset = () => {
        setDragState(prev => ({
            ...prev,
            offset: INITIAL_OFFSET,
            lastOffset: INITIAL_OFFSET
        }))
    }


    const onDragStart = (pos: Pos2D) => {
        setDragState(prev => ({
            ...prev,
            dragState: {
                start: pos,
                current: pos
            }
        }));
    };

    const onDragging = (pos: Pos2D) => {
        setDragState(prev => {
            if (!prev.dragState) {
                return {
                    ...prev
                }
            }
            const updatedDragState = {
                start: prev.dragState.start,
                current: pos
            }
            const updatedOffset = {
                x: prev.lastOffset.x + updatedDragState.current.x - updatedDragState.start.x,
                y: prev.lastOffset.y + updatedDragState.current.y - updatedDragState.start.y
            }
            return {
                ...prev,
                offset: updatedOffset,
                dragState: updatedDragState
            }
        })
    }


    const onDragEnd = () => {
        setDragState(prev => {
            return {
                offset: prev.offset,
                lastOffset: prev.offset,
            }
        })
    }


    return {
        eventListeners: {
            mouse: {
                onMouseDown: (e: any) => onDragStart({x: e.clientX, y: e.clientY}),
                //onMouseMove: (e: any) => onDragging({x: e.clientX, y: e.clientY}),
                //onMouseUp: () => onDragEnd(),
            }
        },
        offset: dragState.offset,
        reset: resetOffset
    }
}