namespace Model
    module Model = 
        type Prostozoa(x, y, genome, radius, brightness, accPower, viewWidth, viewDepth, intoxication, maxVelocity) = 
            member this.x = x
            member this.y = y
            member this.genome = genome
            member this.radius = radius
            member this.brightness = brightness
            member this.accPower = accPower    
            member this.viewWidth = viewWidth
            member this.viewDepth = viewDepth
            member this.intoxication = intoxication
            member this.maxVelocity = maxVelocity
    
        type World(toxicity, agressiveness, fertility) =
            let mutable prostozoas = List.empty<Prostozoa>
            member this.toxicity = toxicity
            member this.agressiveness = agressiveness
            member this.fertility = fertility    
            member this.tick time = 
                prostozoas

        let generateRandomGenome motionNeurons interactNeurons = 
            motionNeurons + interactNeurons