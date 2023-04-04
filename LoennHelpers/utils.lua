-- snowberry header
local snowberry_orig_require = require
local require = function(name)
    return snowberry_orig_require("#Snowberry.LoennPluginLoader").EverestRequire(name)
end
-- end snowberry header

local xnaColors = require("consts.xna_colors")
local rectangles = require("structs.rectangle")

local utils = {}

function utils.rectangle(x, y, width, height)
    return rectangles.create(x, y, width, height)
end

function utils.point(x, y)
    return rectangles.create(x, y, 1, 1)
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

-- Using counter clockwise rotation matrix since the Y axis is mirrored
function utils.rotate(x, y, theta)
    return math.cos(theta) * x - y * math.sin(theta), math.sin(theta) * x + math.cos(theta) * y
end

-- Using counter clockwise rotation matrix since the Y axis is mirrored
function utils.rotatePoint(point, theta)
    local x, y = point.x, point.y

    return math.cos(theta) * x - y * math.sin(theta), math.sin(theta) * x + math.cos(theta) * y
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

return utils