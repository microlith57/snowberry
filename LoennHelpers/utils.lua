-- snowberry header
local snowberry_orig_require = require
local require = function(name)
    return snowberry_orig_require("#Snowberry.Editor.LoennInterop.LoennShims").EverestRequire(name)
end
-- end snowberry header

local xnaColors = require("consts.xna_colors")
local rectangles = require("structs.rectangle")
local bit = require("bit")

local utils = {}

function utils.rectangle(x, y, width, height)
    return rectangles.create(x, y, width, height)
end

function utils.point(x, y)
    return rectangles.create(x, y, 1, 1)
end

function utils.rectangleBounds(rectangles)
    local tlx, tly = math.huge, math.huge
    local brx, bry = -math.huge, -math.huge

    for i, rect in ipairs(rectangles) do
        tlx = math.min(tlx, rect.x)
        tly = math.min(tly, rect.y)

        brx = math.max(brx, rect.x + rect.width)
        bry = math.max(bry, rect.y + rect.height)
    end

    return tlx, tly, brx, bry
end

function utils.coverRectangles(rectangles)
    local tlx, tly, brx, bry = utils.rectangleBounds(rectangles)

    return tlx, tly, brx - tlx, bry - tly
end

-- Does the bounds and covering manually, reduces table construction
function utils.coverTriangle(x1, y1, x2, y2, x3, y3)
    local tlx = math.min(x1, x2, x3)
    local tly = math.min(y1, y2, y3)
    local brx = math.max(x1, x2, x3)
    local bry = math.max(y1, y2, y3)

    return tlx, tly, brx - tlx, bry - tly
end

function utils.titleCase(name)
    return name:gsub("(%a)(%a*)", function(a, b) return string.upper(a) .. b end)
end

function utils.parseHexColor(color)
    color = color:match("^#?([0-9a-fA-F]+)$")

    if color then
        if #color == 6 then
            local number = tonumber(color, 16)
            local r, g, b = math.floor(number / 256^2) % 256, math.floor(number / 256) % 256, math.floor(number) % 256

            return true, r / 255, g / 255, b / 255

        elseif #color == 8 then
            local number = tonumber(color, 16)
            local r, g, b = math.floor(number / 256^3) % 256, math.floor(number / 256^2) % 256, math.floor(number / 256) % 256
            local a = math.floor(number) % 256

            return true, r / 255, g / 255, b / 255, a / 255
        end
    end

    return false, 0, 0, 0
end

function utils.rgbToHex(r, g, b)
    local r8 = math.floor(r * 255 + 0.5)
    local g8 = math.floor(g * 255 + 0.5)
    local b8 = math.floor(b * 255 + 0.5)
    local number = r8 * 256^2 + g8 * 256 + b8

    return string.format("%06x", number)
end

function utils.rgbaToHex(r, g, b, a)
    local r8 = math.floor(r * 255 + 0.5)
    local g8 = math.floor(g * 255 + 0.5)
    local b8 = math.floor(b * 255 + 0.5)
    local a8 = math.floor(a * 255 + 0.5)
    local number = r8 * 256^3 + g8 * 256^2 + b8 * 256 + a8

    return string.format("%08x", number)
end

-- Based on implementation from Love2d wiki
function utils.hsvToRgb(h, s, v)
    if s <= 0 then
        return v, v, v
    end

    h = h * 6

    local c = v * s
    local x = (1 - math.abs((h % 2) - 1)) * c
    local m, r, g, b = v - c, 0, 0, 0

    if h < 1 then
        r, g, b = c, x, 0

    elseif h < 2 then
        r, g, b = x, c, 0

    elseif h < 3 then
        r, g, b = 0, c, x

    elseif h < 4 then
        r, g, b = 0, x, c

    elseif h < 5 then
        r, g, b = x, 0, c

    else
        r, g, b = c, 0, x
    end

    return r + m, g + m, b + m
end

-- Based on implementation from internet
function utils.rgbToHsv(r, g, b)
    local hue, saturation, value

    local minimumValue = math.min(r, g, b)
    local maximumValue = math.max(r, g, b)
    local deltaValue = maximumValue - minimumValue

    if minimumValue == maximumValue then
        hue = 0

    elseif r == maximumValue then
        hue = (60 / 360) * (g - b) / deltaValue + 360 / 360

    elseif g == maximumValue then
        hue = (60 / 360) * (b - r) / deltaValue + 120 / 360

    else
        hue = (60 / 360) * (r - g) / deltaValue + 240 / 360
    end

    -- Make sure hue is not negative or above 1
    hue = (hue + 1) % 1

    if maximumValue == 0 then
        saturation = 0

    else
        saturation = deltaValue / maximumValue
    end

    value = maximumValue

    return hue, saturation, value
end

function utils.getXNAColor(name)
    name = name:lower()

    for cName, c in pairs(xnaColors) do
        if cName:lower() == name then
            return c, cName
        end
    end

    return false, false
end

function utils.getColor(color)
    if type(color) == "string" then
        local xnaColor = utils.getXNAColor(color)
        if xnaColor then
            return xnaColor
        end

        local success, r, g, b = utils.parseHexColor(color)
        if success then
            return {r, g, b}
        end
        return success

    elseif type(color) == "table" and (#color == 3 or #color == 4) then
        return color
    end

    return {1, 1, 1}
end

function utils.sameColor(color1, color2)
    if color1 == color2 then
        return true
    end

    if color1 and color2 and color1[1] == color2[1] and color1[2] == color2[2] and color1[3] == color2[3] and color1[4] == color2[4] then
        return true
    end

    return false
end


-- Using counter clockwise rotation matrix since the Y axis is mirrored
function utils.rotate(x, y, theta)
    return math.cos(theta) * x - y * math.sin(theta), math.sin(theta) * x + math.cos(theta) * y
end

-- Using counter clockwise rotation matrix since the Y axis is mirrored
function utils.rotatePoint(point, theta)
    local x, y = point.x, point.y

    return math.cos(theta) * x - y * math.sin(theta), math.sin(theta) * x + math.cos(theta) * y
end

function utils.aabbCheck(r1, r2)
    return not (r2.x >= r1.x + r1.width or r2.x + r2.width <= r1.x or r2.y >= r1.y + r1.height or r2.y + r2.height <= r1.y)
end

function utils.aabbCheckInline(x1, y1, w1, h1, x2, y2, w2, h2)
    return not (x2 >= x1 + w1 or x2 + w2 <= x1 or y2 >= y1 + h1 or y2 + h2 <= y1)
end

function utils.onRectangleBorder(point, rect, threshold)
    return utils.onRectangleBorderInline(point.x, point.y, rect.x, rect.y, rect.width, rect.height, threshold)
end

function utils.onRectangleBorderInline(px, py, rx, ry, rw, rh, threshold)
    threshold = threshold or 0

    local onHorizontal = rx - threshold <= px and px <= rx + rw + threshold
    local onVertical = ry - threshold <= py and py <= ry + rh + threshold

    local onTop = ry - threshold <= py and py <= ry + threshold
    local onBottom = ry + rh - threshold <= py and py <= ry + rh + threshold
    local onLeft = rx - threshold <= px and px <= rx + threshold
    local onRight = rx + rw - threshold <= px and px <= rx + rw + threshold

    local directionHorizontal = 0
    local directionVertical = 0

    if onHorizontal then
        directionVertical = (onTop and -1) or (onBottom and 1) or 0
    end

    if onVertical then
        directionHorizontal = (onLeft and -1) or (onRight and 1) or 0
    end

    local horizontalMatch = onHorizontal
    local verticalMatch = onVertical

    return directionHorizontal ~= 0 or directionVertical ~= 0, directionHorizontal, directionVertical
end

function utils.intersection(r1, r2)
    local x = math.max(r1.x, r2.x)
    local y = math.max(r1.y, r2.y)
    local width = math.min(r1.x + r1.width, r2.x + r2.width) - x
    local height = math.min(r1.y + r1.height, r2.y + r2.height) - y

    return width > 0 and height > 0 and utils.rectangle(x, y, width, height) or nil
end

function utils.intersectionBounds(r1, r2)
    local tlx = math.max(r1.x, r2.x)
    local tly = math.max(r1.y, r2.y)
    local brx = math.min(r1.x + r1.width, r2.x + r2.width)
    local bry = math.min(r1.y + r1.height, r2.y + r2.height)

    return tlx, tly, brx, bry
end

function utils.subtractRectangle(r1, r2)
    local tlx, tly, brx, bry = utils.intersectionBounds(r1, r2)

    if tlx >= brx or tly >= bry  then
        -- No intersection
        return {r1}
    end

    local remaining = {}

    -- Left rectangle
    if tlx > r1.x then
        table.insert(remaining, utils.rectangle(r1.x, r1.y, tlx - r1.x, r1.height))
    end

    -- Right rectangle
    if brx < r1.x + r1.width then
        table.insert(remaining, utils.rectangle(brx, r1.y, r1.x + r1.width - brx, r1.height))
    end

    -- Top rectangle
    if tly > r1.y then
        table.insert(remaining, utils.rectangle(tlx, r1.y, brx - tlx, tly - r1.y))
    end

    -- Bottom rectangle
    if bry < r1.y + r1.height then
        table.insert(remaining, utils.rectangle(tlx, bry, brx - tlx, r1.y + r1.height - bry))
    end

    return remaining
end

function utils.logn(base, n)
    return math.log(n) / math.log(base)
end

function utils.setRandomSeed(v)
    if type(v) == "number" then
        math.randomseed(v)

    elseif type(v) == "string" and #v >= 1 then
        local s = string.byte(v, 1)

        for i = 2, #v do
            s = s * 256
            s = s + string.byte(v)
        end

        math.randomseed(s)
    end
end

function utils.distanceSquared(x1, y1, x2, y2)
    local deltaX = x1 - x2
    local deltaY = y1 - y2

    return deltaX * deltaX + deltaY * deltaY
end

function utils.distance(x1, y1, x2, y2)
    return math.sqrt(utils.distanceSquared(x1, y1, x2, y2))
end

function utils.getSimpleCoordinateSeed(x, y)
    return math.abs(bit.lshift(x, math.ceil(utils.logn(2, math.abs(y) + 1)))) + math.abs(y)
end

function utils.setSimpleCoordinateSeed(x, y)
    local seed = utils.getSimpleCoordinateSeed(x, y)

    utils.setRandomSeed(seed)
end

function utils.deepcopy(v, copyMetatables)
    if type(v) == "table" then
        local res = {}

        if copyMetatables ~= false then
            setmetatable(res, getmetatable(v))
        end

        for key, value in pairs(v) do
            res[key] = utils.deepcopy(value)
        end

        return res

    else
        return v
    end
end

function utils.shuffle(t)
    for i = #t, 2, -1 do
        local j = math.random(i)

        t[i], t[j] = t[j], t[i]
    end
end

-- Shallow mode doesn't check table values recursively
function utils.equals(lhs, rhs, shallow)
    if lhs == rhs then
        return true
    end

    local lhsType = type(lhs)
    local rhsType = type(rhs)

    if lhsType ~= rhsType then
        return false
    end

    if lhsType == "table" then
        local equalFunc = shallow and (function(a, b)
            return a == b
        end) or utils.equals

        for k, v in pairs(lhs) do
            if not equalFunc(rhs[k], v) then
                return false
            end
        end

        for k, v in pairs(rhs) do
            if not equalFunc(lhs[k], v) then
                return false
            end
        end

        return true
    end

    return false
end

function utils.clamp(value, min, max)
    return math.min(math.max(value, min), max)
end

function utils.round(n, decimals)
    if decimals and decimals > 0 then
        local pow = 10^decimals

        return math.floor(n * pow + 0.5) / pow

    else
        return math.floor(n + 0.5)
    end
end

function utils.isCallable(f)
    local argType = type(f)

    if argType == "function" then
        return true

    elseif argType == "table" then
        local metatable = getmetatable(f)

        if metatable and metatable.__call then
            return true
        end
    end

    return false
end

function utils.callIfFunction(f, ...)
    if utils.isCallable(f) then
        return f(...)
    end

    return f
end

function utils.filter(predicate, data)
    local res = {}

    for _, v in ipairs(data) do
        if predicate(v) then
            table.insert(res, v)
        end
    end

    return res
end

function utils.contains(value, data)
    for _, dataValue in pairs(data) do
        if value == dataValue then
            return true
        end
    end

    return false
end

function utils.unique(data, hashFunc)
    hashFunc = hashFunc or function(value) return value end

    local unique = {}
    local seen = {}

    for _, value in ipairs(data) do
        local hash = hashFunc(value)

        if not seen[hash] then
            table.insert(unique, value)

            seen[hash] = true
        end
    end

    return unique
end

function utils.getPath(data, path, default, createIfMissing)
    local target = data

    for i, part in ipairs(path) do
        local lastPart = i == #path
        local newTarget = target[part]

        if newTarget ~= nil then
            target = newTarget

        else
            if createIfMissing then
                if not lastPart then
                    target[part] = {}
                    target = target[part]

                else
                    target[part] = default
                    target = default
                end

            else
                return default
            end
        end
    end

    return target
end

function utils.setPath(data, path, value, createIfMissing)
    local target = data

    for i, part in ipairs(path) do
        local lastPart = i == #path

        if lastPart then
            target[part] = value

        else
            local newTarget = target[part]

            if newTarget then
                target = newTarget

            else
                if createIfMissing then
                    target[part] = {}
                    target = target[part]

                else
                    return false
                end
            end
        end
    end

    return true
end

-- moved from entities.lua
function utils.getDrawableRectangle(drawables)
    if #drawables == 0 and drawables.getRectangle then
        return drawables:getRectangle()
    end

    -- TODO - Inline coverRectangles?
    -- Check if this is expensive enough in larger rooms

    local rectangles = {}

    for i, drawable in ipairs(drawables) do
        if drawable.getRectangle then
            rectangles[i] = drawable:getRectangle()

            if drawable.ignoreRest then
                break
            end
        end
    end

    local x, y, width, height = utils.coverRectangles(rectangles)

    return utils.rectangle(x, y, width, height)
end

return utils