local drawing = {}

function drawing.getCurvePoint(start, stop, control, percent)
    local startMul = (1 - percent)^2
    local controlMul = 2 * (1 - percent) * percent
    local stopMul = percent^2

    local x = start[1] * startMul + control[1] * controlMul + stop[1] * stopMul
    local y = start[2] * startMul + control[2] * controlMul + stop[2] * stopMul

    return x, y
end

function drawing.getSimpleCurve(start, stop, control, resolution)
    control = control or {(start[1] + stop[1]) / 2, (start[2] + stop[2]) / 2}
    resolution = resolution or 16

    local res = {}

    for i = 0, resolution do
        local x, y = drawing.getCurvePoint(start, stop, control, i / resolution)

        table.insert(res, x)
        table.insert(res, y)
    end

    return res
end

return drawing